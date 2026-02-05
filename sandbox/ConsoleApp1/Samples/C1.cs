using System.ComponentModel;

namespace ConsoleApp1.Samples;

internal class C1
{
#nullable disable
    public event EventHandler MyEvent1;
#nullable restore
    public event EventHandler? MyEvent2;
    public event CancelEventHandler? MyCancelEvent1;
}
