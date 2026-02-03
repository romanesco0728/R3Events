仕様: Events.R3.R3EventAttribute と自動生成の振る舞い

1. 目的

この仕様は、Incremental Source Generator が生成する属性型 `Events.R3.R3EventAttribute` と、その属性を利用した拡張メソッド自動生成の振る舞いを定義します。ジェネレータは、利用側の型が持つ public イベントを検出し、R3 の `global::R3.Observable<T>` を返す拡張メソッドを生成します。

2. 属性: Events.R3.R3EventAttribute

- 種別: `global::System.Attribute` を継承する public の sealed クラス
- 名前空間: `Events.R3`（生成コードで namespace に合わせる）
- コンストラクタ: `public R3EventAttribute(global::System.Type type)` — 対象の型を指定
- プロパティ: `public global::System.Type Type { get; }`
- ルール: 生成コード内では公開 API 型参照に必ず `global::` プレフィックスを付与する（global-prefix.instructions.md 準拠）

3. ジェネレータのトリガー

- 利用側プロジェクトで任意の static partial クラスに `Events.R3.R3EventAttribute` を付与する。
  例: `[Events.R3.R3EventAttribute(typeof(C1))] internal static partial class C1Extensions;`
- 上記があると、ジェネレータは型 `C1` の public イベントを解析し、拡張メソッドを生成する。

4. 生成される拡張メソッドの形

- 各 public event に対して、次の命名ルールでメソッドを生成する: `{EventName}AsObservable`
- メソッドのシグネチャ:
  - public static global::R3.Observable<T> {EventName}AsObservable(this global::NamespaceOfTarget.TargetType instance, global::System.Threading.CancellationToken cancellationToken = default)
  - T は次のルールで決定する:
    - EventHandler または EventHandler<global::System.EventArgs> 系列のイベントは `global::R3.Unit` を返す Observable。
    - CancelEventHandler や他の EventHandler<TEventArgs> 系列は TEventArgs を返す Observable（nullable をそのまま維持）。
    - 非 EventHandler 系のデリゲートは、そのデリゲートの最後の引数型（EventArgs 系）を T とする。もし推定できない場合は `global::System.Object` を使用する。
- 生成されるメソッド本体は、R3 のユーティリティを使った実装を行う。例:
  - `Observable.FromEventHandler` を使う場合や、`Observable.FromEvent<TDelegate, TPayload>` を使う場合がある。
  - 必要に応じて `AsUnitObservable()` や `Select(ep => ep.Args)` のような変換を行う。

5. 例外と安全性

- 生成コードは null 許容参照型を尊重する。
- 生成コードでは公開 API に `global::` プレフィックスを用いる。
- 生成コードはファイルスコープの namespace を使用し、XML ドキュメントコメントを付与する。

6. 依存関係

- 生成コードは `global::R3` 名前空間の型（主に `Observable<T>`, `Unit`, `Observable` ユーティリティ）に依存する。
- 必要であれば、生成コード内に using を書くが、公開 API 参照は `global::` を使用する。

7. テスト

- ジェネレータのユニットテストは、入力ソースコードを与えて期待される生成コードが出力されることを検証する。
- イベントの多様なケース（null 許容、Generic EventHandler<T>, カスタムデリゲート）をカバーすること。
