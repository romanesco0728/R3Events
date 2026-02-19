使用例と生成コードの例

例 1: 単純な EventHandler (非ジェネリック属性)

入力ソース:

```csharp
class C1
{
    public event EventHandler? MyEvent1;
}

[R3Events.R3EventAttribute(typeof(C1))]
internal static partial class C1Extensions;
```

生成されるコード:

```csharp
namespace R3Events.Generated
{
    using global::System;
    using global::System.Threading;

    internal static partial class C1Extensions
    {
        /// <summary>
        /// Returns an Observable that signals a unit value when <c>MyEvent1</c> is raised.
        /// </summary>
        public static global::R3.Observable<global::R3.Unit> MyEvent1AsObservable(this global::C1 c1, global::System.Threading.CancellationToken cancellationToken = default)
        {
            var rawObservable = global::R3.Observable.FromEventHandler(
                h => c1.MyEvent1 += h,
                h => c1.MyEvent1 -= h,
                cancellationToken
                );
            return rawObservable.AsUnitObservable();
        }
    }
}
```

例 1.5: 単純な EventHandler (ジェネリック属性、C# 11+ のみ)

入力ソース:

```csharp
class C1
{
    public event EventHandler? MyEvent1;
}

// C# 11 以降では、型パラメータで対象型を指定できる
[R3Events.R3EventAttribute<C1>]
internal static partial class C1Extensions;
```

生成されるコード:

（例 1 と同じコードが生成される）

例 2: CancelEventHandler

入力ソース:

```csharp
class C1
{
    public event CancelEventHandler? MyEvent2;
}

[R3Events.R3EventAttribute(typeof(C1))]
internal static partial class C1Extensions;
```

生成されるコード:

```csharp
namespace R3Events.Generated
{
    using global::System;
    using global::System.ComponentModel;
    using global::System.Threading;

    internal static partial class C1Extensions
    {
        /// <summary>
        /// Returns an Observable of CancelEventArgs when <c>MyEvent2</c> is raised.
        /// </summary>
        public static global::R3.Observable<global::System.ComponentModel.CancelEventArgs> MyEvent2AsObservable(this global::C1 c1, global::System.Threading.CancellationToken cancellationToken = default)
        {
            var rawObservable = global::R3.Observable.FromEvent<global::System.ComponentModel.CancelEventHandler, (object?, global::System.ComponentModel.CancelEventArgs Args)>(
                static h => new global::System.ComponentModel.CancelEventHandler((s, e) => h((s, e))),
                h => c1.MyEvent2 += h,
                h => c1.MyEvent2 -= h,
                cancellationToken
                );
            return rawObservable.Select(ep => ep.Args);
        }
    }
}
```
