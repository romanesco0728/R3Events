#pragma warning disable CS0067
using R3;
using System.ComponentModel;
using Events.R3;
using TestApp;

namespace TestApp
{
    class C1
    {
        public event EventHandler? MyEvent1;
        public event CancelEventHandler? MyEvent2;
    }

    // Test the new generic attribute syntax (C# 11+)
    [R3Event<C1>]
    static partial class C1Extensions
    {
    }

    // Also test that the old syntax still works
    class C2
    {
        public event EventHandler? MyEvent3;
    }

    [R3Event(typeof(C2))]
    static partial class C2Extensions
    {
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("Testing generic attribute support...");

            // Create instances and test
            var c1 = new C1();
            var c2 = new C2();

            // Use the generated extension methods
            var subscription1 = c1.MyEvent1AsObservable().Subscribe(_ => Console.WriteLine("Event1 fired"));
            var subscription2 = c2.MyEvent3AsObservable().Subscribe(_ => Console.WriteLine("Event3 fired"));

            Console.WriteLine("Both attribute variants work!");
        }
    }
}
