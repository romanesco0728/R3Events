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

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for event-added scenario");
        reasons[1][0].Reasons.ShouldBe("Modified", "Second run should be tracked as Modified when an event is added to the target class");
    }

    [TestMethod]
    public void TargetClassEventRemoved_ShouldBeTrackedAsModified()
    {
        // lang=C#-test
        var step1 = """
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

        // lang=C#-test
        var step2 = """
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

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("R3Events.", step1, step2);

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for event-removed scenario");
        reasons[1][0].Reasons.ShouldBe("Modified", "Second run should be tracked as Modified when an event is removed from the target class");
    }

    [TestMethod]
    public void TargetClassEventPayloadTypeChanged_ShouldBeTrackedAsModified()
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
    public event System.EventHandler<int>? NameChanged;
}

[R3Events.R3Event<Person>]
public static partial class PersonExtensions
{
}
""";

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("R3Events.", step1, step2);

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for event payload-type change scenario");
        reasons[1][0].Reasons.ShouldBe("Modified", "Second run should be tracked as Modified when an event payload type changes on the target class");
    }

    [TestMethod]
    public void InputUnchanged_ShouldBeTrackedAsUnchanged()
    {
        // lang=C#-test
        var step = """
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

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("R3Events.", step, step);

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for unchanged-input scenario");
        reasons[1][0].Reasons.ShouldBe("Unchanged", "Second run should be tracked as Unchanged when the input source is identical");
    }
}
