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

    [TestMethod]
    public void ExtensionClassRemovedStaticModifier_ShouldBeTrackedAsModified()
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
}

[R3Events.R3Event<Person>]
public partial class PersonExtensions
{
}
""";

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("R3Events.", step1, step2);

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for static-modifier change scenario");
        reasons[1][0].Reasons.ShouldBe("Modified", "Second run should be tracked as Modified when static modifier is removed and diagnostic condition changes");
    }

    [TestMethod]
    public void ExtensionClassRemovedPartialModifier_ShouldBeTrackedAsModified()
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
}

[R3Events.R3Event<Person>]
public static class PersonExtensions
{
}
""";

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("R3Events.", step1, step2);

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for partial-modifier change scenario");
        reasons[1][0].Reasons.ShouldBe("Modified", "Second run should be tracked as Modified when partial modifier is removed and diagnostic condition changes");
    }

    [TestMethod]
    public void ExtensionClassBecameGeneric_ShouldBeTrackedAsModified()
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
}

[R3Events.R3Event<Person>]
public static partial class PersonExtensions<T>
{
}
""";

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("R3Events.", step1, step2);

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for generic-arity change scenario");
        reasons[1][0].Reasons.ShouldBe("Modified", "Second run should be tracked as Modified when class becomes generic and diagnostic condition changes");
    }

    [TestMethod]
    public void ExtensionClassBecameNested_ShouldBeTrackedAsModified()
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
}

public static class Outer
{
    [R3Events.R3Event<Person>]
    public static partial class PersonExtensions
    {
    }
}
""";

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("R3Events.", step1, step2);

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for nesting change scenario");
        reasons[1][0].Reasons.ShouldBe("Modified", "Second run should be tracked as Modified when class becomes nested and diagnostic condition changes");
    }

    [TestMethod]
    public void R3EventTargetTypeChanged_ShouldBeTrackedAsModified()
    {
        // lang=C#-test
        var step1 = """
namespace IncrementalTest;

public class Person
{
    public event System.EventHandler<string>? NameChanged;
}

public class Employee
{
    public event System.EventHandler<string>? DepartmentChanged;
}

[R3Events.R3Event<Person>]
public static partial class Extensions
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

public class Employee
{
    public event System.EventHandler<string>? DepartmentChanged;
}

[R3Events.R3Event<Employee>]
public static partial class Extensions
{
}
""";

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("R3Events.", step1, step2);

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for R3Event target type change scenario");
        reasons[1][0].Reasons.ShouldBe("Modified", "Second run should be tracked as Modified when R3Event target type changes");
    }

    [TestMethod]
    public void TargetClassEventNameChanged_ShouldBeTrackedAsModified()
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
    public event System.EventHandler<string>? DisplayNameChanged;
}

[R3Events.R3Event<Person>]
public static partial class PersonExtensions
{
}
""";

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("R3Events.", step1, step2);

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for event-name change scenario");
        reasons[1][0].Reasons.ShouldBe("Modified", "Second run should be tracked as Modified when target class event name changes");
    }

    [TestMethod]
    public void TargetClassEventOrderChangedOnly_ShouldBeTrackedAsUnchanged()
    {
        // lang=C#-test
        var step1 = """
namespace IncrementalTest;

public class Person
{
    public event System.EventHandler<int>? AgeChanged;
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

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for event-order-only change scenario");
        reasons[1][0].Reasons.ShouldBe("Unchanged", "Second run should be tracked as Unchanged when only event declaration order changes");
    }
}
