using R3EventsGenerator.Tests.ModernLang.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;

namespace R3EventsGenerator.Tests.ModernLang;

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

        // When C# 10 is used, the generic attribute syntax ([R3Event<T>]) is not supported
        // by the language, so using it should cause a compilation error (CS8936).
        // Note: R3EventAttribute<T> exists in the shared attributes assembly regardless of
        // language version, but C# 10 does not allow the generic attribute usage syntax.
        var result = CSharpGeneratorRunner.RunGenerator(source, preprocessorSymbols: ["NET6_0_OR_GREATER"], languageVersion: LanguageVersion.CSharp10);

        // We expect an error because generic attribute syntax requires C# 11 or later
        var errors = result.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
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

    [TestMethod]
    public void GeneratedSource_ShouldUseGlobalQualifiedTypesInCode_AndUserFacingTypesInXml()
    {
        // lang=C#-test
        var source = """
namespace GenericTest;

public sealed class PayloadModel
{
}

public class TestClass
{
    public event System.EventHandler<PayloadModel>? ValueChanged;
}

[R3Events.R3Event<TestClass>]
internal static partial class TestExtensions
{
}
""";

        var generatedSources = CSharpGeneratorRunner.RunGeneratorAndGetGeneratedSources(source, preprocessorSymbols: ["NET7_0_OR_GREATER"]);
        var extensionSource = generatedSources.Single(s => s.Contains("ValueChangedAsObservable", StringComparison.Ordinal));

        extensionSource.ShouldContain("global::GenericTest.TestClass");
        extensionSource.ShouldContain("global::GenericTest.PayloadModel");
        extensionSource.ShouldContain("<see cref=\"GenericTest.PayloadModel\"/>");
        extensionSource.ShouldNotContain("<see cref=\"global::");
    }

    [TestMethod]
    public void GlobalNamespace_WithNullableEnabled_ShouldNotProduceNullableAnnotationWarnings()
    {
        // lang=C#-test
        // Reproduces the ConsoleApp4 scenario: nullable-enabled project, class in global namespace.
        // Previously the generated file emitted #nullable disable while still containing System.Object?,
        // which triggered CS8669 for auto-generated code.
        var source = """
public class TestClass
{
    public event System.EventHandler? MyEvent;
    public event System.EventHandler<string>? ValueChanged;
}

[R3Events.R3Event<TestClass>]
public static partial class TestExtensions
{
}
""";

        var result = CSharpGeneratorRunner.RunGenerator(
            source,
            preprocessorSymbols: ["NET7_0_OR_GREATER"],
            // NullableContextOptions.Enable matches <Nullable>enable</Nullable> — the exact ConsoleApp4 scenario
            nullableContextOptions: Microsoft.CodeAnalysis.NullableContextOptions.Enable);

        var nullableWarnings = result
            .Where(d => d.Id is "CS8632" or "CS8669")
            .ToArray();
        nullableWarnings.ShouldBeEmpty(
            $"Generated code in global namespace should not produce nullable annotation warnings, but got: {string.Join(", ", nullableWarnings.Select(d => $"{d.Id}: {d.GetMessage()}"))}");
    }

    [TestMethod]
    public void GlobalNamespace_GeneratedSource_ShouldContainNullableEnableDirective()
    {
        // lang=C#-test
        // Verify that the generated file for a global-namespace class always declares #nullable enable,
        // ensuring nullable annotations in the generated code are valid in any consumer project.
        // Uses EventHandler<T> to exercise the non-unit path that emits (global::System.Object?, T Args).
        var source = """
public class TestClass
{
    public event System.EventHandler<string>? ValueChanged;
}

[R3Events.R3Event<TestClass>]
public static partial class TestExtensions
{
}
""";

        var generatedSources = CSharpGeneratorRunner.RunGeneratorAndGetGeneratedSources(
            source,
            preprocessorSymbols: ["NET7_0_OR_GREATER"]);

        generatedSources.ShouldNotBeEmpty("Generator should produce at least one source file");
        var extensionSource = generatedSources.Single(s => s.Contains("ValueChangedAsObservable", StringComparison.Ordinal));
        // Must declare #nullable enable — the generated body uses System.Object? as the event sender type
        extensionSource.ShouldContain("#nullable enable");
        extensionSource.ShouldContain("global::System.Object?");
    }
}
