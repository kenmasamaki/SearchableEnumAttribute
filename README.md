# SearchableEnumAttribute

`SearchableEnumAttribute` は、Unity のインスペクター上で Enum 型の選択肢を検索可能にするカスタム属性です。大量の Enum 定数がある場合でも、検索機能により素早く目的の値を選択できます。

## 特徴

- Unity インスペクターで Enum フィールドを検索可能に
- 既存の Enum 型に簡単に適用可能
- 大規模な Enum 定数で効率的に選択
- 並び替え（元順・アルファベット順・逆順）に対応
- 検索モード（部分一致・前方一致）を切り替え可能
- カテゴリ・グループ化表示に対応（例: `ATACK_01` なら `ATACK` でグループ化）
- 項目数・絞込数・選択インデックスの表示
- 検索語の強調表示

## 使用例

public enum ActionType
{
	ATACK_01,
	ATACK_02,
	ATACK_03,
	ATACK_04,
	SPECIAL_01,
	SPECIAL_02,
	SPECIAL_03,
	SPECIAL_04,
	DEFENSE_01,
	DEFENSE_02,
	DEFENSE_03,
	DEFENSE_04,
	HEAL_01,
	HEAL_02,
	HEAL_03,
	HEAL_04,
}

[SearchableEnum]
public ActionType actionType;


上記のように、`[SearchableEnum]` 属性を Enum フィールドに付与するだけで、インスペクター上で Enum の選択肢が検索可能になります。
