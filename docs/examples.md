# Usage Examples and Generated Code

## Example 1: Simple EventHandler (non-generic attribute)

Input source:

```csharp
class C1
{
    public event EventHandler? MyEvent1;
}

[R3Events.R3EventAttribute(typeof(C1))]
internal static partial class C1Extensions;
```

Generated code:

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

## Example 1.5: Simple EventHandler (generic attribute, C# 11+ only)

Input source:

```csharp
class C1
{
    public event EventHandler? MyEvent1;
}

// C# 11 and later: specify the target type via a type parameter
[R3Events.R3EventAttribute<C1>]
internal static partial class C1Extensions;
```

Generated code:

(Same output as Example 1)

## Example 2: CancelEventHandler

Input source:

```csharp
class C1
{
    public event CancelEventHandler? MyEvent2;
}

[R3Events.R3EventAttribute(typeof(C1))]
internal static partial class C1Extensions;
```

Generated code:

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
