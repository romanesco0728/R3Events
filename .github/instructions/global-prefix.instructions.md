---
description: 'Guideline to always use global:: fully-qualified type names in generated C# code to reduce symbol collisions.'
applyTo: '**/*.cs'
---

# 生成コードにおける global:: プレフィックス使用ガイド

## 目的
ソースジェネレータが出力するコードが、利用側プロジェクトの型や using ディレクティブと衝突するリスクを下げるため、生成コード内では可能な限り `global::` プレフィックス付きの完全修飾名を使うことを必須ルールとします。

## ルール
- 生成される C# ファイル内で参照するすべての型、名前空間、属性、例外、インターフェイス等は `global::` を付けた完全修飾名で記述すること。
    - 例: `global::System.Type`、`global::R3Events.R3EventAttribute`、`global::MyCompany.Project.Models.User`。
- 生成コード内で `using` を使う場合でも、外部依存や公開 API に使われる型参照は `global::` を用いた完全修飾名に置き換えることを推奨する。
- 例外: 非公開なローカルヘルパークラス（ジェネレータ内部のみで完結するもの）で読みやすさのためにローカル using を使う場合は例外とする。ただし公開 API や出力される型には必ず `global::` を使うこと。

## 理由
- 利用側プロジェクトで同名の型や型エイリアス、using エイリアスが定義されていると、生成コードがコンパイルエラーや意図しない型解決を引き起こす可能性があるため。
- `global::` を使うことで CLR 上の確定した型を参照でき、名前解決による衝突を回避できる。

## 実装例
```csharp
// 良い例（推奨）
var code = @"namespace R3Events
{
    using global::System;

    internal sealed class R3EventAttribute : global::System.Attribute
    {
        public R3EventAttribute(global::System.Type type)
        {
            this.Type = type ?? throw new global::System.ArgumentNullException(nameof(type));
        }

        public global::System.Type Type { get; }
    }
}"
;

// 悪い例（競合しやすい）
var code = @"namespace R3Events
{
    using System;

    internal sealed class R3EventAttribute : Attribute
    {
        public R3EventAttribute(Type type) { this.Type = type; }
        public Type Type { get; }
    }
}"
;
```

## チェックリスト（生成時）
- [ ] 生成コード内のすべての公開 API 型参照に `global::` が付いているか。
- [ ] 例外的に `using` を使っている箇所は内部専用で外部 API を公開していないか。
- [ ] XML ドキュメントやコメント内で参照している型についても必要に応じて `global::` を使っているか。

## 備考
- このルールは自動生成コードの安定性を高める目的であり、コーディングスタイルの一部として今後追加のジェネレータ実装にも適用してください。
