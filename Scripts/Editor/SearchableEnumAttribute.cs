using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
#endif

public class SearchableEnumAttribute : PropertyAttribute
{
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(SearchableEnumAttribute))]
public class SearchableEnumDrawer : PropertyDrawer
{
	private string _enumName;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		if (property.type != "Enum")
		{
			EditorGUI.PropertyField(position, property, label);
			return;
		}

		try
		{
			_enumName = property.enumNames[property.enumValueIndex];
		}
		catch
		{
			_enumName = "Invalid Index";
		}

		var newRect = EditorGUI.PrefixLabel(position, label);
		if (GUI.Button(newRect, _enumName, EditorStyles.popup))
		{
			Popup.Show(newRect, property);
		}
	}

	private class Popup : PopupWindowContent
	{
		private readonly SearchField _searchField;
		private readonly PopupTreeView _treeView;
		private readonly float _height;
		private string[] _enumNames;

		// 並び替えモード
		private enum SortMode { Original, Alphabetical, Reverse }
		private SortMode _sortMode = SortMode.Original;

		// 検索モード
		private enum SearchMode { Partial, Prefix }
		private SearchMode _searchMode = SearchMode.Partial;

		// グループ化表示フラグ
		private bool _useGrouping = false;

		public static void Show(Rect position, SerializedProperty property)
		{
			PopupWindow.Show(position, new Popup(property));
		}

		private Popup(SerializedProperty property)
		{
			_enumNames = property.enumNames.ToArray(); // ここでキャッシュ
			_height = GetHeight(_enumNames) + 170f; // 引数変更

			_treeView = new PopupTreeView(new TreeViewState(), this, property, _enumNames, () => _sortMode, () => _searchMode, () => _useGrouping);
			_searchField = new SearchField();
			_searchField.downOrUpArrowKeyPressed += _treeView.SetFocusAndEnsureSelectedItem;
			_treeView.Reload();
			_searchField.SetFocus();
		}

		public override void OnGUI(Rect rect)
		{
			// 並び替えラベル
			Rect sortLabelRect = rect;
			sortLabelRect.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.LabelField(sortLabelRect, "並び替え");

			// 並び替えツールバー
			Rect sortRect = rect;
			sortRect.y += EditorGUIUtility.singleLineHeight;
			sortRect.height = EditorGUIUtility.singleLineHeight;
			int selected = (int)_sortMode;
			string[] options = { "元順", "アルファベット順", "逆順" };
			int newSelected = GUI.Toolbar(sortRect, selected, options);
			if (newSelected != selected)
			{
				_sortMode = (SortMode)newSelected;
				_treeView.ClearSelection();
				_treeView.Reload();
			}

			// 検索モードラベル
			Rect searchLabelRect = rect;
			searchLabelRect.y += EditorGUIUtility.singleLineHeight * 2;
			searchLabelRect.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.LabelField(searchLabelRect, "検索モード");

			// 検索モードツールバー
			Rect searchRect = rect;
			searchRect.y += EditorGUIUtility.singleLineHeight * 3;
			searchRect.height = EditorGUIUtility.singleLineHeight;
			int searchSelected = (int)_searchMode;
			string[] searchOptions = { "部分一致", "前方一致" };
			int newSearchSelected = GUI.Toolbar(searchRect, searchSelected, searchOptions);
			if (newSearchSelected != searchSelected)
			{
				_searchMode = (SearchMode)newSearchSelected;
				_treeView.SetSearchMode(_searchMode);
				_treeView.ClearSelection();
				_treeView.Reload();
			}

			// グループ化表示トグル
			Rect groupRect = rect;
			groupRect.y += EditorGUIUtility.singleLineHeight * 4;
			groupRect.height = EditorGUIUtility.singleLineHeight;
			bool newGrouping = EditorGUI.ToggleLeft(groupRect, "カテゴリ・グループ化して表示", _useGrouping);
			if (newGrouping != _useGrouping)
			{
				_useGrouping = newGrouping;
				_treeView.ClearSelection();
				_treeView.Reload();
			}

			// 項目数・選択インデックス表示
			Rect infoRect = rect;
			infoRect.y += EditorGUIUtility.singleLineHeight * 5;
			infoRect.height = EditorGUIUtility.singleLineHeight;
			var enumCount = _treeView.GetEnumCount();
			var filteredCount = _treeView.GetFilteredCount();
			int selectedIndex = _treeView.GetSelectedIndex();
			string selectedText = selectedIndex >= 0
				? $"（{selectedIndex + 1} / {filteredCount}）"
				: "";
			EditorGUI.LabelField(infoRect, $"全項目数: {enumCount}　絞込数: {filteredCount}　選択中: {selectedText}");

			// 検索フィールド
			Rect searchFieldRect = rect;
			searchFieldRect.y += EditorGUIUtility.singleLineHeight * 6;
			searchFieldRect.height = EditorGUIUtility.singleLineHeight;
			_treeView.searchString = _searchField.OnGUI(searchFieldRect, _treeView.searchString);

			// TreeView
			Rect treeRect = rect;
			treeRect.y += EditorGUIUtility.singleLineHeight * 7;
			treeRect.height = GetWindowSize().y - EditorGUIUtility.singleLineHeight * 7;
			_treeView.OnGUI(treeRect);
		}

		public override Vector2 GetWindowSize()
		{
			return new Vector2(250f, _height);
		}

		private static float GetHeight(string[] enumNames)
		{
			return Mathf.Min(enumNames.Length, 8) * EditorGUIUtility.singleLineHeight;
		}

		public override void OnClose()
		{
			// キャッシュ解放
			_enumNames = null;
			base.OnClose();
		}

		// PopupTreeView クラスに BuildRoot() の実装を修正
		private class PopupTreeView : TreeView
		{
			private readonly SerializedProperty _property;
			private readonly Popup _popup;
			private readonly string[] _enumNames; // 追加
			private readonly Func<SortMode> _getSortMode;
			private readonly Func<SearchMode> _getSearchMode;
			private readonly Func<bool> _getGrouping;
			private int _hoveredId = -1;

			private SearchMode _searchMode = SearchMode.Partial;

			private int _filteredCount = 0;
			public int GetFilteredCount() => _filteredCount;

			public PopupTreeView(TreeViewState state, Popup popup, SerializedProperty property, string[] enumNames, Func<SortMode> getSortMode, Func<SearchMode> getSearchMode, Func<bool> getGrouping)
				: base(state)
			{
				_property = property;
				_popup = popup;
				_enumNames = enumNames; // 追加
				_getSortMode = getSortMode;
				_getSearchMode = getSearchMode;
				_getGrouping = getGrouping;
			}

			public void SetSearchMode(SearchMode mode) => _searchMode = mode;

			protected override TreeViewItem BuildRoot()
			{
				var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };
				var children = new List<TreeViewItem>();

				string[] enumNames = _enumNames; // ここをキャッシュ参照
				switch (_getSortMode())
				{
					case SortMode.Alphabetical:
						enumNames = enumNames.OrderBy(x => x).ToArray();
						break;
					case SortMode.Reverse:
						enumNames = enumNames.Reverse().ToArray();
						break;
				}

				var search = searchString?.ToLowerInvariant();
				bool useGrouping = _getGrouping();

				if (useGrouping)
				{
					// グループ化
					var groupDict = new Dictionary<string, List<(int, string)>>();
					for (int i = 0; i < enumNames.Length; i++)
					{
						var name = enumNames[i];
						var split = name.Split('_');
						string group = split.Length > 1 ? split[0] : "その他";
						if (!groupDict.ContainsKey(group))
							groupDict[group] = new List<(int, string)>();
						groupDict[group].Add((i, name));
					}

					foreach (var kv in groupDict)
					{
						var groupNode = new TreeViewItem { id = 10000 + kv.Key.GetHashCode(), depth = 0, displayName = kv.Key };
						var groupChildren = new List<TreeViewItem>();
						foreach (var (idx, name) in kv.Value)
						{
							if (!string.IsNullOrEmpty(search))
							{
								var lower = name.ToLowerInvariant();
								bool match = _getSearchMode() switch
								{
									SearchMode.Partial => lower.Contains(search),
									SearchMode.Prefix => lower.StartsWith(search),
									_ => true
								};
								if (!match) continue;
							}
							groupChildren.Add(new TreeViewItem { id = idx, depth = 1, displayName = name });
						}
						if (groupChildren.Count > 0)
						{
							groupNode.children = groupChildren;
							children.Add(groupNode);
						}
					}
					_filteredCount = children.Sum(g => g.children?.Count ?? 0);
				}
				else
				{
					// グループ化しない場合
					_filteredCount = 0;
					for (int i = 0; i < enumNames.Length; i++)
					{
						var name = enumNames[i];
						if (!string.IsNullOrEmpty(search))
						{
							var lower = name.ToLowerInvariant();
							bool match = _getSearchMode() switch
							{
								SearchMode.Partial => lower.Contains(search),
								SearchMode.Prefix => lower.StartsWith(search),
								_ => true
							};
							if (!match) continue;
						}
						children.Add(new TreeViewItem { id = i, depth = 0, displayName = name });
						_filteredCount++;
					}
				}

				root.children = children;
				return root;
			}


			protected override void RowGUI(RowGUIArgs args)
			{
				bool isGroupNode = args.item.depth == 0 && args.item.hasChildren;
				if (!isGroupNode && Event.current.type == EventType.MouseDown && Event.current.button == 0 && args.rowRect.Contains(Event.current.mousePosition))
				{
					OnItemClicked(args.item.id);
					Event.current.Use();
					return;
				}

				// 親項目（グループノード）は選択不可
				if (args.rowRect.Contains(Event.current.mousePosition) && !isGroupNode)
				{
					_hoveredId = args.item.id;
					var prevColor = GUI.color;
					GUI.color = new Color(0.7f, 0.85f, 1f, 1f); // 薄い青
					EditorGUI.DrawRect(args.rowRect, GUI.color);
					GUI.color = prevColor;

					if (state.selectedIDs.Count != 1 || state.selectedIDs[0] != args.item.id)
					{
						state.selectedIDs.Clear();
						state.selectedIDs.Add(args.item.id);
						Repaint();
					}
				}
				else if (_hoveredId == args.item.id && Event.current.type == EventType.Repaint)
				{
					_hoveredId = -1;
				}

				// 強調表示付き描画
				var labelRect = args.rowRect;
				string displayName = args.item.displayName;
				string search = searchString?.ToLowerInvariant();

				if (!string.IsNullOrEmpty(search))
				{
					int matchIndex = displayName.ToLowerInvariant().IndexOf(search);
					if (matchIndex >= 0)
					{
						var before = displayName.Substring(0, matchIndex);
						var match = displayName.Substring(matchIndex, search.Length);
						var after = displayName.Substring(matchIndex + search.Length);

						var style = EditorStyles.label;
						EditorGUI.LabelField(labelRect, before, style);
						var matchSize = style.CalcSize(new GUIContent(before));
						labelRect.x += matchSize.x;
						var highlightStyle = new GUIStyle(style);
						highlightStyle.normal.textColor = Color.yellow;
						EditorGUI.LabelField(labelRect, match, highlightStyle);
						var matchSize2 = style.CalcSize(new GUIContent(match));
						labelRect.x += matchSize2.x;
						EditorGUI.LabelField(labelRect, after, style);
						return;
					}
				}

				base.RowGUI(args);
			}

			public int GetEnumCount()
			{
				return _enumNames.Length; // キャッシュ参照
			}

			// PopupTreeView クラス
			public int GetSelectedIndex()
			{
				if (state.selectedIDs.Count == 1)
				{
					var selectedId = state.selectedIDs[0];
					var selectedItem = FindItem(selectedId, rootItem);
					// 親項目（グループノード）は選択扱いしない
					if (selectedItem != null && !(selectedItem.depth == 0 && selectedItem.hasChildren))
						return selectedId;
				}
				return -1;
			}

			// PopupTreeView クラス内に OnItemClicked メソッドを追加
			private void OnItemClicked(int index)
			{
				_property.enumValueIndex = index;
				_property.serializedObject.ApplyModifiedProperties();
				_popup.editorWindow?.Close();
			}

			public void ClearSelection()
			{
				state.selectedIDs.Clear();
			}
		}
	}
}

#endif