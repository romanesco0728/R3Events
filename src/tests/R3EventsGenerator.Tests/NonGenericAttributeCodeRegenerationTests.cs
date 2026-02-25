using R3EventsGenerator.Tests.Utilities;
using Shouldly;

namespace R3EventsGenerator.Tests;

[TestClass]
public class NonGenericAttributeCodeRegenerationTests
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

[R3Events.R3Event(typeof(Person))]
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

[R3Events.R3Event(typeof(Person))]
public static partial class PersonExtensions
{
}
""";

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("R3Events.", step1, step2);

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for non-generic attribute event-added scenario");
        reasons[1][0].Reasons.ShouldBe("Modified", "Second run should be tracked as Modified when an event is added under non-generic attribute usage");
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

[R3Events.R3Event(typeof(Person))]
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

[R3Events.R3Event(typeof(Employee))]
public static partial class Extensions
{
}
""";

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("R3Events.", step1, step2);

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for non-generic R3Event target type change scenario");
        reasons[1][0].Reasons.ShouldBe("Modified", "Second run should be tracked as Modified when non-generic R3Event target type changes");
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

[R3Events.R3Event(typeof(Person))]
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

[R3Events.R3Event(typeof(Person))]
public static partial class PersonExtensions
{
}
""";

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("R3Events.", step1, step2);

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for non-generic attribute event-order-only change scenario");
        reasons[1][0].Reasons.ShouldBe("Unchanged", "Second run should be tracked as Unchanged when only event declaration order changes under non-generic attribute usage");
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

[R3Events.R3Event(typeof(Person))]
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

[R3Events.R3Event(typeof(Person))]
public static partial class PersonExtensions
{
}
""";

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("R3Events.", step1, step2);

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for non-generic attribute event-removed scenario");
        reasons[1][0].Reasons.ShouldBe("Modified", "Second run should be tracked as Modified when an event is removed under non-generic attribute usage");
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

[R3Events.R3Event(typeof(Person))]
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

[R3Events.R3Event(typeof(Person))]
public static partial class PersonExtensions
{
}
""";

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("R3Events.", step1, step2);

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for non-generic attribute event payload-type change scenario");
        reasons[1][0].Reasons.ShouldBe("Modified", "Second run should be tracked as Modified when an event payload type changes under non-generic attribute usage");
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

[R3Events.R3Event(typeof(Person))]
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

[R3Events.R3Event(typeof(Person))]
public static partial class PersonExtensions
{
}
""";

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("R3Events.", step1, step2);

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for non-generic attribute event-name change scenario");
        reasons[1][0].Reasons.ShouldBe("Modified", "Second run should be tracked as Modified when target class event name changes under non-generic attribute usage");
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

[R3Events.R3Event(typeof(Person))]
public static partial class PersonExtensions
{
}
""";

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("R3Events.", step, step);

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for non-generic unchanged-input scenario");
        reasons[1][0].Reasons.ShouldBe("Unchanged", "Second run should be tracked as Unchanged when the non-generic input source is identical");
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

[R3Events.R3Event(typeof(Person))]
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

[R3Events.R3Event(typeof(Person))]
public partial class PersonExtensions
{
}
""";

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("R3Events.", step1, step2);

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for non-generic static-modifier change scenario");
        reasons[1][0].Reasons.ShouldBe("Modified", "Second run should be tracked as Modified when static modifier is removed under non-generic attribute usage");
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

[R3Events.R3Event(typeof(Person))]
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

[R3Events.R3Event(typeof(Person))]
public static class PersonExtensions
{
}
""";

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("R3Events.", step1, step2);

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for non-generic partial-modifier change scenario");
        reasons[1][0].Reasons.ShouldBe("Modified", "Second run should be tracked as Modified when partial modifier is removed under non-generic attribute usage");
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

[R3Events.R3Event(typeof(Person))]
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

[R3Events.R3Event(typeof(Person))]
public static partial class PersonExtensions<T>
{
}
""";

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("R3Events.", step1, step2);

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for non-generic generic-arity change scenario");
        reasons[1][0].Reasons.ShouldBe("Modified", "Second run should be tracked as Modified when class becomes generic under non-generic attribute usage");
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

[R3Events.R3Event(typeof(Person))]
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
    [R3Events.R3Event(typeof(Person))]
    public static partial class PersonExtensions
    {
    }
}
""";

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("R3Events.", step1, step2);

        reasons[0][0].Reasons.ShouldBe("New", "First run should be tracked as New for non-generic nesting change scenario");
        reasons[1][0].Reasons.ShouldBe("Modified", "Second run should be tracked as Modified when class becomes nested under non-generic attribute usage");
    }
}
