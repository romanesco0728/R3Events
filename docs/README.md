Incremental Source Generator — Events → R3 Observable

概要

このプロジェクトのソースジェネレータは、利用側の型が定義する public イベントを自動的に global::R3.Observable<T> を返す拡張メソッドに変換するためのボイラープレートを生成します。生成は、利用側で任意の static partial クラスに `R3Events.R3EventAttribute` を付与することでトリガーされます。

目的

- イベント購読のボイラープレートを削減する。
- 型安全な Observable API を自動生成する。

このドキュメント配下には、仕様、使用例、及びジェネレータ出力ルールが含まれます。

ファイル一覧

- `docs/spec.md` - 仕様詳細（診断 R3W001 および コードフィックスの仕様を含む）
- `docs/examples.md` - 利用例と生成されるコードの例
- `docs/generator-guidelines.md` - 生成コードの命名・スタイル・実装上の注意点
