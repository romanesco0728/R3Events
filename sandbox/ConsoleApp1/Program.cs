// See https://aka.ms/new-console-template for more information
using Events.R3;
using System.ComponentModel;

Console.WriteLine("Hello, World!");

[R3Event(typeof(C1))]
internal static partial class C1Ext;

class C1
{
    public event EventHandler? MyEvent1;
    //public event CancelEventHandler? MyEvent2;
}
