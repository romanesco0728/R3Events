using System.ComponentModel;

namespace ConsoleApp1.Samples;

internal class C1
{
#nullable disable
    public event EventHandler MyEvent1;
    public event EventHandler<CancelEventArgs> MyGenericEvent1;
    public event CancelEventHandler MyCancelEvent1;

    // 出力対象外
    public static event EventHandler MyStaticEvent1;
#nullable restore
    public event EventHandler? MyEvent2;
    public event EventHandler<CancelEventArgs>? MyGenericEvent2;
    public event CancelEventHandler? MyCancelEvent2;

    // 出力対象外
    public static event EventHandler? MyStaticEvent2;
}
