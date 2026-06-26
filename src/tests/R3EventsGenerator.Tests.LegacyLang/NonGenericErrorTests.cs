using R3EventsGenerator.Tests.LegacyLang.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Linq;

namespace R3EventsGenerator.Tests.LegacyLang
{
    [TestClass]
    public sealed class NonGenericErrorTests
    {
        [TestMethod]
        public void NonGenericAttribute_OnLegacyLanguage_ShouldNotProduceErrors()
        {
            // lang=C#-test
            var source = @"
namespace WarnTest
{

public class TestClass
{
    public event System.EventHandler? MyEvent;
}

[R3Events.R3Event(typeof(TestClass))]
internal static partial class TestExtensions
{
}
}
";

            var result = CSharpGeneratorRunner.RunGenerator(source);

            var errors = result.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.ShouldBeEmpty("Generator should not produce errors when non-generic attribute is used in legacy-language project");
        }

        [TestMethod]
        public void R3EventOnNonPartialClass_ShouldProduceER001Error()
        {
            // lang=C#-test
            var source = @"
namespace ErrorTest
{

[R3Events.R3Event(typeof(int))]
public static class IntExtensions
{
}
}
";

            var result = CSharpGeneratorRunner.RunGenerator(source);

            result.Length.ShouldBe(1, "Generator should produce exactly one diagnostic");
            result[0].Id.ShouldBe("R3E001", "Diagnostic ID should be R3E001 for non-partial class error");
            result[0].GetMessage().ShouldContain("ErrorTest.IntExtensions");
            result[0].GetMessage().ShouldNotContain("global::");
        }

        [TestMethod]
        public void R3EventOnNestedClass_ShouldProduceER002Error()
        {
            // lang=C#-test
            var source = @"
namespace ErrorTest
{

public static class OuterClass
{
    [R3Events.R3Event(typeof(int))]
    public static partial class IntExtensions
    {
    }
}
}
";

            var result = CSharpGeneratorRunner.RunGenerator(source);

            result.Length.ShouldBe(1, "Generator should produce exactly one diagnostic");
            result[0].Id.ShouldBe("R3E002", "Diagnostic ID should be R3E002 for nested class error");
            result[0].GetMessage().ShouldContain("ErrorTest.OuterClass.IntExtensions");
            result[0].GetMessage().ShouldNotContain("global::");
        }

        [TestMethod]
        public void R3EventOnNonStaticClass_ShouldProduceER003Error()
        {
            // lang=C#-test
            var source = @"
namespace ErrorTest
{

[R3Events.R3Event(typeof(int))]
public partial class IntExtensions
{
}
}
";

            var result = CSharpGeneratorRunner.RunGenerator(source);

            result.Length.ShouldBe(1, "Generator should produce exactly one diagnostic");
            result[0].Id.ShouldBe("R3E003", "Diagnostic ID should be R3E003 for non-static class error");
            result[0].GetMessage().ShouldContain("ErrorTest.IntExtensions");
            result[0].GetMessage().ShouldNotContain("global::");
        }

        [TestMethod]
        public void R3EventOnGenericClass_ShouldProduceER004Error()
        {
            // lang=C#-test
            var source = @"
namespace ErrorTest
{

[R3Events.R3Event(typeof(int))]
public static partial class IntExtensions<T>
{
}
}
";

            var result = CSharpGeneratorRunner.RunGenerator(source);

            result.Length.ShouldBe(1, "Generator should produce exactly one diagnostic");
            result[0].Id.ShouldBe("R3E004", "Diagnostic ID should be R3E004 for generic class error");
            result[0].GetMessage().ShouldContain("ErrorTest.IntExtensions<T>");
            result[0].GetMessage().ShouldNotContain("global::");
        }

        [TestMethod]
        public void GlobalNamespace_WithNullableEnabled_ShouldNotProduceNullableAnnotationWarnings()
        {
            // lang=C#-test
            // Verifies that a non-generic attribute usage in a global-namespace class with a nullable-enabled project
            // does not produce CS8632 or CS8669. Previously the generated file emitted #nullable disable while
            // containing System.Object?, triggering CS8669 for auto-generated code.
            var source = @"
public class TestClass
{
    public event System.EventHandler MyEvent;
    public event System.EventHandler<string> ValueChanged;
}

[R3Events.R3Event(typeof(TestClass))]
public static partial class TestExtensions
{
}
";

            var result = CSharpGeneratorRunner.RunGenerator(
                source,
                nullableContextOptions: Microsoft.CodeAnalysis.NullableContextOptions.Annotations);

            var nullableWarnings = result
                .Where(d => d.Id == "CS8632" || d.Id == "CS8669")
                .ToArray();
            nullableWarnings.ShouldBeEmpty(
                "Generated code in global namespace should not produce nullable annotation warnings, but got: " +
                string.Join(", ", nullableWarnings.Select(d => d.Id + ": " + d.GetMessage())));
        }

        [TestMethod]
        public void GlobalNamespace_WithNullableDisabled_ShouldNotProduceNullableAnnotationWarnings()
        {
            // lang=C#-test
            // Verifies legacy (nullable-disabled) projects using a global-namespace class do not get
            // CS8632/CS8669 from the generated file.
            var source = @"
public class TestClass
{
    public event System.EventHandler MyEvent;
}

[R3Events.R3Event(typeof(TestClass))]
public static partial class TestExtensions
{
}
";

            var result = CSharpGeneratorRunner.RunGenerator(source);

            var nullableWarnings = result
                .Where(d => d.Id == "CS8632" || d.Id == "CS8669")
                .ToArray();
            nullableWarnings.ShouldBeEmpty(
                "Generated code in global namespace should not produce nullable annotation warnings in nullable-disabled projects, but got: " +
                string.Join(", ", nullableWarnings.Select(d => d.Id + ": " + d.GetMessage())));
        }

        [TestMethod]
        public void GlobalNamespace_GeneratedSource_ShouldContainNullableEnableDirective()
        {
            // lang=C#-test
            // Verify the generated file for a global-namespace class always declares #nullable enable,
            // ensuring nullable annotations (e.g., System.Object?) are valid regardless of the consumer's setting.
            var source = @"
public class TestClass
{
    public event System.EventHandler MyEvent;
}

[R3Events.R3Event(typeof(TestClass))]
public static partial class TestExtensions
{
}
";

            var generatedSources = CSharpGeneratorRunner.RunGeneratorAndGetGeneratedSources(source);

            generatedSources.ShouldNotBeEmpty("Generator should produce at least one source file");
            var extensionSource = generatedSources.Single(s => s.Contains("MyEventAsObservable"));
            // Must declare #nullable enable so that System.Object? (sender type) is valid in any consumer project
            extensionSource.ShouldContain("#nullable enable");
        }

        [TestMethod]
        public void ObsoleteWarningEvent_GeneratedSource_ShouldCopyObsoleteAttribute()
        {
            // lang=C#-test
            var source = @"
namespace ObsoleteWarnTest
{

public class TestClass
{
    [System.Obsolete(""Use NewEvent instead"", false)]
    public event System.EventHandler MyEvent;
}

[R3Events.R3Event(typeof(TestClass))]
public static partial class TestExtensions
{
}
}
";

            var generatedSources = CSharpGeneratorRunner.RunGeneratorAndGetGeneratedSources(source);

            generatedSources.ShouldNotBeEmpty("Generator should produce source for obsolete events as well as non-obsolete events");
            var extensionSource = generatedSources.Single(s => s.Contains("MyEventAsObservable"));
            extensionSource.ShouldContain("[global::System.Obsolete(\"Use NewEvent instead\", false)]");
        }

        [TestMethod]
        public void ObsoleteWarningEvent_UsingGeneratedMethod_ShouldReportSingleConsumerWarning()
        {
            // lang=C#-test
            var source = @"
namespace ObsoleteWarnTest
{

public class TestClass
{
    [System.Obsolete(""Use NewEvent instead"", false)]
    public event System.EventHandler MyEvent;
}

[R3Events.R3Event(typeof(TestClass))]
public static partial class TestExtensions
{
}

public static class Consumer
{
    public static void Subscribe(TestClass instance)
    {
        instance.MyEventAsObservable();
    }
}
}
";

            var result = CSharpGeneratorRunner.RunGenerator(source);

            var obsoleteDiagnostics = result.Where(d => d.Id == "CS0618" || d.Id == "CS0619").ToArray();
            obsoleteDiagnostics.ShouldHaveSingleItem("Only the consumer call site should report obsolete usage for warning-level events");
            obsoleteDiagnostics[0].Id.ShouldBe("CS0618", "Warning-level obsolete events should remain warnings on the generated observable method");
            obsoleteDiagnostics[0].GetMessage().ShouldContain("MyEventAsObservable");
        }

        [TestMethod]
        public void ObsoleteErrorEvent_UsingGeneratedMethod_ShouldReportSingleConsumerError()
        {
            // lang=C#-test
            var source = @"
namespace ObsoleteErrorTest
{

public class TestClass
{
    [System.Obsolete(""Use NewEvent instead"", true)]
    public event System.EventHandler MyEvent;
}

[R3Events.R3Event(typeof(TestClass))]
public static partial class TestExtensions
{
}

public static class Consumer
{
    public static void Subscribe(TestClass instance)
    {
        instance.MyEventAsObservable();
    }
}
}
";

            var result = CSharpGeneratorRunner.RunGenerator(source);

            var obsoleteDiagnostics = result.Where(d => d.Id == "CS0618" || d.Id == "CS0619").ToArray();
            obsoleteDiagnostics.ShouldHaveSingleItem("Only the consumer call site should report obsolete usage for error-level events");
            obsoleteDiagnostics[0].Id.ShouldBe("CS0619", "Error-level obsolete events should remain errors on the generated observable method");
            obsoleteDiagnostics[0].GetMessage().ShouldContain("MyEventAsObservable");
        }

        [TestMethod]
        public void ObsoleteNoArgEvent_GeneratedSource_ShouldCopyObsoleteAttribute()
        {
            // lang=C#-test
            var source = @"
namespace ObsoleteNoArgTest
{

public class TestClass
{
    [System.Obsolete]
    public event System.EventHandler MyEvent;
}

[R3Events.R3Event(typeof(TestClass))]
public static partial class TestExtensions
{
}
}
";

            var generatedSources = CSharpGeneratorRunner.RunGeneratorAndGetGeneratedSources(source);

            generatedSources.ShouldNotBeEmpty("Generator should produce source for obsolete events as well as non-obsolete events");
            var extensionSource = generatedSources.Single(s => s.Contains("MyEventAsObservable"));
            extensionSource.ShouldContain("[global::System.Obsolete]");
        }
    }
}
