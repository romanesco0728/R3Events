using R3EventsGenerator.Tests.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;

namespace R3EventsGenerator.Tests;

[TestClass]
public sealed class NonGenericAttributeTests
{
    [TestMethod]
    public void NonGenericAttribute_ShouldAlwaysBeGenerated()
    {
        // lang=C#-test
        var source = """
namespace GenericTest;

public class TestClass
{
    public event System.EventHandler? MyEvent;
}

[R3Events.R3Event(typeof(TestClass))]
internal static partial class TestExtensions
{
}
""";

        // Test with C# 10
        var result = CSharpGeneratorRunner.RunGenerator(source, preprocessorSymbols: ["NET6_0_OR_GREATER"], languageVersion: LanguageVersion.CSharp10);

        // Should not have any errors
        var errors = result.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        errors.ShouldBeEmpty($"Non-generic attribute should work with C# 10, but got: {string.Join(", ", errors.Select(e => e.GetMessage()))}");
    }
}
