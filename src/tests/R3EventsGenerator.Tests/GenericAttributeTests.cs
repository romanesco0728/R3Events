using R3EventsGenerator.Tests.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;

namespace R3EventsGenerator.Tests;

[TestClass]
public sealed class GenericAttributeTests
{
    [TestMethod]
    public void GenericAttribute_ShouldBeGeneratedForCSharp11()
    {
        // lang=C#-test
        var source = """
namespace GenericTest;

#nullable enable

public class TestClass
{
    public event System.EventHandler? MyEvent;
}

[R3Events.R3Event<TestClass>]
internal static partial class TestExtensions
{
}
""";

        var result = CSharpGeneratorRunner.RunGenerator(source, preprocessorSymbols: ["NET7_0_OR_GREATER"]);

        // Should not have any errors
        var errors = result.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        errors.ShouldBeEmpty($"Should not have errors, but got: {string.Join(", ", errors.Select(e => e.GetMessage()))}");
    }

    [TestMethod]
    public void GenericAttribute_ShouldNotBeGeneratedForCSharp10()
    {
        // lang=C#-test
        var source = """
namespace GenericTest;

public class TestClass
{
    public event System.EventHandler? MyEvent;
}

[R3Events.R3Event<TestClass>]
internal static partial class TestExtensions
{
}
""";

        // Parse with C# 10
        var result = CSharpGeneratorRunner.RunGenerator(source, preprocessorSymbols: ["NET6_0_OR_GREATER"], languageVersion: LanguageVersion.CSharp10);

        // When C# 10 is used, the generic attribute type should not exist,
        // so using it should cause a compilation error
        var errors = result.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();

        // We expect an error because R3EventAttribute<T> is not generated for C# 10
        errors.ShouldNotBeEmpty("Should have errors when using generic attribute with C# 10");
    }

    [TestMethod]
    public void GenericAttribute_ShouldGenerateCorrectExtensionMethods()
    {
        // lang=C#-test
        var source = """
namespace GenericTest;

public class TestClass
{
    public event System.EventHandler? MyEvent;
    public event System.EventHandler<int>? ValueChanged;
}

[R3Events.R3Event<TestClass>]
internal static partial class TestExtensions
{
}
""";

        var result = CSharpGeneratorRunner.RunGenerator(source, preprocessorSymbols: ["NET7_0_OR_GREATER"]);

        // Should not have any errors
        var errors = result.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        errors.ShouldBeEmpty($"Should not have errors, but got: {string.Join(", ", errors.Select(e => e.GetMessage()))}");
    }

    [TestMethod]
    public void BothAttributeVariants_CanCoexistInSameProject()
    {
        // lang=C#-test
        var source = """
namespace CoexistTest;

public class TestClass1
{
    public event System.EventHandler? Event1;
}

public class TestClass2
{
    public event System.EventHandler? Event2;
}

// Using non-generic attribute
[R3Events.R3Event(typeof(TestClass1))]
internal static partial class TestExtensions1
{
}

// Using generic attribute
[R3Events.R3Event<TestClass2>]
internal static partial class TestExtensions2
{
}
""";

        var result = CSharpGeneratorRunner.RunGenerator(source, preprocessorSymbols: ["NET7_0_OR_GREATER"]);

        // Should not have any errors
        var errors = result.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        errors.ShouldBeEmpty($"Both attribute variants should work together, but got: {string.Join(", ", errors.Select(e => e.GetMessage()))}");
    }
}
