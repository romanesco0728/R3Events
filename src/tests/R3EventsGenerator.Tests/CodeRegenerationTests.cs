using R3EventsGenerator.Tests.Utilities;
using Shouldly;

namespace R3EventsGenerator.Tests;

[TestClass]
public class CodeRegenerationTests
{
    [TestMethod]
    public void TargetClassEventAdded_ShouldBeTrackedAsModified()
    {
        // lang=C#-test
        var step1 = """
namespace IncrementalTest;

public class Person
{
    public event System.EventHandler<string>? NameChanged;
}

[R3Events.R3Event<Person>]
public static partial class PersonExtensions
{
}
""";

        // lang=C#-test
        var step2 = """
namespace IncrementalTest;

public class Person
{
    public event System.EventHandler<string>? NameChanged;
    public event System.EventHandler<int>? AgeChanged;
}

[R3Events.R3Event<Person>]
public static partial class PersonExtensions
{
}
""";

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("R3Events.", step1, step2);

        reasons[0][0].Reasons.ShouldBe("New");
        reasons[1][0].Reasons.ShouldBe("Modified");
    }
}
