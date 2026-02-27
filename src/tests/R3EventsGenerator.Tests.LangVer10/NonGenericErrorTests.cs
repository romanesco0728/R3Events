using R3EventsGenerator.Tests.LangVer10.Utilities;
using Microsoft.CodeAnalysis;
using Shouldly;

namespace R3EventsGenerator.Tests.LangVer10;

[TestClass]
public sealed class NonGenericErrorTests
{
    [TestMethod]
    public void NonGenericAttribute_OnCSharp10_ShouldNotProduceErrors()
    {
        // lang=C#-test
        var source = @"
namespace WarnTest;

public class TestClass
{
    public event System.EventHandler? MyEvent;
}

[R3Events.R3Event(typeof(TestClass))]
internal static partial class TestExtensions
{
}
";

        var result = CSharpGeneratorRunner.RunGenerator(source);

        var errors = result.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        errors.ShouldBeEmpty("Generator should not produce errors when non-generic attribute is used in C# 10 project");
    }

    [TestMethod]
    public void R3EventOnNonPartialClass_ShouldProduceER001Error()
    {
        // lang=C#-test
        var source = @"
namespace ErrorTest;

[R3Events.R3Event(typeof(int))]
public static class IntExtensions
{
}
";

        var result = CSharpGeneratorRunner.RunGenerator(source);

        result.Length.ShouldBe(1, "Generator should produce exactly one diagnostic");
        result[0].Id.ShouldBe("R3E001", "Diagnostic ID should be R3E001 for non-partial class error");
    }

    [TestMethod]
    public void R3EventOnNestedClass_ShouldProduceER002Error()
    {
        // lang=C#-test
        var source = @"
namespace ErrorTest;

public static class OuterClass
{
    [R3Events.R3Event(typeof(int))]
    public static partial class IntExtensions
    {
    }
}
";

        var result = CSharpGeneratorRunner.RunGenerator(source);

        result.Length.ShouldBe(1, "Generator should produce exactly one diagnostic");
        result[0].Id.ShouldBe("R3E002", "Diagnostic ID should be R3E002 for nested class error");
    }

    [TestMethod]
    public void R3EventOnNonStaticClass_ShouldProduceER003Error()
    {
        // lang=C#-test
        var source = @"
namespace ErrorTest;

[R3Events.R3Event(typeof(int))]
public partial class IntExtensions
{
}
";

        var result = CSharpGeneratorRunner.RunGenerator(source);

        result.Length.ShouldBe(1, "Generator should produce exactly one diagnostic");
        result[0].Id.ShouldBe("R3E003", "Diagnostic ID should be R3E003 for non-static class error");
    }

    [TestMethod]
    public void R3EventOnGenericClass_ShouldProduceER004Error()
    {
        // lang=C#-test
        var source = @"
namespace ErrorTest;

[R3Events.R3Event(typeof(int))]
public static partial class IntExtensions<T>
{
}
";

        var result = CSharpGeneratorRunner.RunGenerator(source);

        result.Length.ShouldBe(1, "Generator should produce exactly one diagnostic");
        result[0].Id.ShouldBe("R3E004", "Diagnostic ID should be R3E004 for generic class error");
    }
}
