#pragma warning disable CS0067
using R3;
using System.ComponentModel;

Console.WriteLine("Hello, World!");

class C1
{
    public event EventHandler? MyEvent1;
    public event CancelEventHandler? MyEvent2;
}

static class C1Extensions
{
    public static Observable<Unit> MyEvent1AsObservable(this C1 c1, CancellationToken cancellationToken = default)
    {
        var rawObservable = Observable.FromEventHandler(
            h => c1.MyEvent1 += h,
            h => c1.MyEvent1 -= h,
            cancellationToken
            );
        return ObservableExtensions.AsUnitObservable(rawObservable);
    }
    public static Observable<CancelEventArgs> MyEvent2AsObservable(this C1 c1, CancellationToken cancellationToken = default)
    {
        var rawObservable = Observable.FromEvent<CancelEventHandler, (object?, CancelEventArgs Args)>(
            static h => new CancelEventHandler((s, e) => h((s, e))),
            h => c1.MyEvent2 += h,
            h => c1.MyEvent2 -= h,
            cancellationToken
            );
        return ObservableExtensions.Select(rawObservable, ep => ep.Args);
    }
}
