using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using R3EventsGenerator.Tests.Utilities;
using Shouldly;

namespace R3EventsGenerator.Tests;

[TestClass]
public sealed class ErrorTests
{
    [TestMethod]
    public void R3EventOnNonPartialClass_ShouldProduceER001Error()
    {
        // lang=C#-test
        var source = """
namespace ErrorTest;

[R3Events.R3Event(typeof(int))]
public static class IntExtensions
{
}
""";

        var result = CSharpGeneratorRunner.RunGenerator(source);
        var errors = result.Where(static d => d.Descriptor.DefaultSeverity == DiagnosticSeverity.Error).ToArray();

        errors.Length.ShouldBe(1, "Generator should produce exactly one diagnostic");
        errors[0].Id.ShouldBe("R3E001", "Diagnostic ID should be R3E001 for non-partial class error");
    }

    [TestMethod]
    public void R3EventOnNestedClass_ShouldProduceER002Error()
    {
        // lang=C#-test
        var source = """
namespace ErrorTest;

public static class OuterClass
{
    [R3Events.R3Event(typeof(int))]
    public static partial class IntExtensions
    {
    }
}
""";

        var result = CSharpGeneratorRunner.RunGenerator(source);
        var errors = result.Where(static d => d.Descriptor.DefaultSeverity == DiagnosticSeverity.Error).ToArray();

        errors.Length.ShouldBe(1, "Generator should produce exactly one diagnostic");
        errors[0].Id.ShouldBe("R3E002", "Diagnostic ID should be R3E002 for nested class error");
    }

    [TestMethod]
    public void R3EventOnNonStaticClass_ShouldProduceER003Error()
    {
        // lang=C#-test
        var source = """
namespace ErrorTest;

[R3Events.R3Event(typeof(int))]
public partial class IntExtensions
{
}
""";

        var result = CSharpGeneratorRunner.RunGenerator(source);
        var errors = result.Where(static d => d.Descriptor.DefaultSeverity == DiagnosticSeverity.Error).ToArray();

        errors.Length.ShouldBe(1, "Generator should produce exactly one diagnostic");
        errors[0].Id.ShouldBe("R3E003", "Diagnostic ID should be R3E003 for non-static class error");
    }

    [TestMethod]
    public void R3EventOnGenericClass_ShouldProduceER004Error()
    {
        // lang=C#-test
        var source = """
namespace ErrorTest;

[R3Events.R3Event(typeof(int))]
public static partial class IntExtensions<T>
{
}
""";

        var result = CSharpGeneratorRunner.RunGenerator(source);
        var errors = result.Where(static d => d.Descriptor.DefaultSeverity == DiagnosticSeverity.Error).ToArray();

        errors.Length.ShouldBe(1, "Generator should produce exactly one diagnostic");
        errors[0].Id.ShouldBe("R3E004", "Diagnostic ID should be R3E004 for generic class error");
    }

    [TestMethod]
    public void NonGenericAttribute_OnCSharp11_ShouldProduceR3W001Warning()
    {
        // lang=C#-test
        var source = """
namespace WarnTest;

public class TestClass
{
    public event System.EventHandler? MyEvent;
}

[R3Events.R3Event(typeof(TestClass))]
internal static partial class TestExtensions
{
}
""";

        var result = CSharpGeneratorRunner.RunGenerator(source);

        var r3Warnings = result.Where(d => d.Id == "R3W001").ToArray();
        r3Warnings.Length.ShouldBe(1, "Generator should produce exactly one R3W001 warning");
        r3Warnings[0].Id.ShouldBe("R3W001", "Diagnostic ID should be R3W001 when non-generic attribute is used with C# 11+");

        // The warning should point to the attribute node, not the class declaration identifier.
        // In the source above, the attribute is on line 7 (0-based) and the class is on line 8.
        var warningLine = r3Warnings[0].Location.GetLineSpan().StartLinePosition.Line;
        warningLine.ShouldBe(7, "R3W001 warning should be located at the attribute line, not the class declaration line");
    }

    [TestMethod]
    public void NonGenericAttribute_OnCSharp10_ShouldNotProduceWarning()
    {
        // lang=C#-test
        var source = """
namespace WarnTest;

public class TestClass
{
    public event System.EventHandler? MyEvent;
}

[R3Events.R3Event(typeof(TestClass))]
internal static partial class TestExtensions
{
}
""";

        var result = CSharpGeneratorRunner.RunGenerator(source, languageVersion: LanguageVersion.CSharp10);

        var r3Warnings = result.Where(d => d.Id == "R3W001").ToArray();
        r3Warnings.ShouldBeEmpty("Generator should not produce R3W001 warning when using C# 10");
    }
}
