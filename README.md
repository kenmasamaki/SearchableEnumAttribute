# SearchableEnumAttribute

`SearchableEnumAttribute` は、Unity のインスペクター上で Enum 型の選択肢を検索可能にするカスタム属性です。大量の Enum 定数がある場合でも、検索機能により素早く目的の値を選択できます。

![Image](https://github-production-user-asset-6210df.s3.amazonaws.com/124390814/468939325-ee695e97-b204-4fac-ac8b-03125cd141b7.png?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAVCODYLSA53PQK4ZA%2F20250722%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20250722T032415Z&X-Amz-Expires=300&X-Amz-Signature=6e2bb94e33a6ab172a775b6d9edcaa41d1c2c8fafc23e166bc7dad26ed8eb674&X-Amz-SignedHeaders=host)

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

```csharp
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
```


上記のように、`[SearchableEnum]` 属性を Enum フィールドに付与するだけで、インスペクター上で Enum の選択肢が検索可能になります。
