using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using R3EventsGenerator.Tests.ModernLang.Utilities;
using Shouldly;

namespace R3EventsGenerator.Tests.ModernLang;

[TestClass]
public sealed class NonGenericErrorTests
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
        errors[0].GetMessage().ShouldContain("ErrorTest.IntExtensions");
        errors[0].GetMessage().ShouldNotContain("global::");
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
        errors[0].GetMessage().ShouldContain("ErrorTest.OuterClass.IntExtensions");
        errors[0].GetMessage().ShouldNotContain("global::");
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
        errors[0].GetMessage().ShouldContain("ErrorTest.IntExtensions");
        errors[0].GetMessage().ShouldNotContain("global::");
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
        errors[0].GetMessage().ShouldContain("ErrorTest.IntExtensions<T>");
        errors[0].GetMessage().ShouldNotContain("global::");
    }

    [TestMethod]
    public void NonGenericAttribute_OnCSharp11_ShouldProduceR3I001Info()
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

        var r3Infos = result.Where(d => d.Id == "R3I001").ToArray();
        r3Infos.Length.ShouldBe(1, "Generator should produce exactly one R3I001 info diagnostic");
        r3Infos[0].Id.ShouldBe("R3I001", "Diagnostic ID should be R3I001 when non-generic attribute is used with C# 11+");
        r3Infos[0].GetMessage().ShouldContain("WarnTest.TestExtensions");
        r3Infos[0].GetMessage().ShouldNotContain("global::");

        // The info diagnostic should point to the attribute node, not the class declaration identifier.
        // In the source above, the attribute is on line 7 (0-based) and the class is on line 8.
        var infoLine = r3Infos[0].Location.GetLineSpan().StartLinePosition.Line;
        infoLine.ShouldBe(7, "R3I001 info should be located at the attribute line, not the class declaration line");
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

        var errors = result.Where(static d => d.Descriptor.DefaultSeverity == DiagnosticSeverity.Error).ToArray();
        errors.ShouldBeEmpty("Generator should not produce errors when non-generic attribute is used with C# 10");

        var r3Infos = result.Where(d => d.Id == "R3I001").ToArray();
        r3Infos.ShouldBeEmpty("Generator should not produce R3I001 info when using C# 10");
    }

    [TestMethod]
    public void ObsoleteWarningEvent_GeneratedSource_ShouldCopyObsoleteAttribute()
    {
        // lang=C#-test
        var source = """
namespace ObsoleteWarnTest;

public class TestClass
{
    [System.Obsolete("Use NewEvent instead", false)]
    public event System.EventHandler MyEvent;
}

[R3Events.R3Event(typeof(TestClass))]
public static partial class TestExtensions
{
}
""";

        var generatedSources = CSharpGeneratorRunner.RunGeneratorAndGetGeneratedSources(source);

        generatedSources.ShouldNotBeEmpty("Generator should produce source for obsolete events as well as non-obsolete events");
        var extensionSource = generatedSources.Single(s => s.Contains("MyEventAsObservable"));
        extensionSource.ShouldContain("[global::System.Obsolete(\"Use NewEvent instead\", false)]");
    }

    [TestMethod]
    public void ObsoleteWarningEvent_UsingGeneratedMethod_ShouldReportSingleConsumerWarning()
    {
        // lang=C#-test
        var source = """
namespace ObsoleteWarnTest;

public class TestClass
{
    [System.Obsolete("Use NewEvent instead", false)]
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
""";

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
        var source = """
namespace ObsoleteErrorTest;

public class TestClass
{
    [System.Obsolete("Use NewEvent instead", true)]
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
""";

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
        var source = """
namespace ObsoleteNoArgTest;

public class TestClass
{
    [System.Obsolete]
    public event System.EventHandler MyEvent;
}

[R3Events.R3Event(typeof(TestClass))]
public static partial class TestExtensions
{
}
""";

        var generatedSources = CSharpGeneratorRunner.RunGeneratorAndGetGeneratedSources(source);

        generatedSources.ShouldNotBeEmpty("Generator should produce source for obsolete events as well as non-obsolete events");
        var extensionSource = generatedSources.Single(s => s.Contains("MyEventAsObservable"));
        extensionSource.ShouldContain("[global::System.Obsolete]");
    }

    [TestMethod]
    public void ObsoleteMessageOnlyEvent_GeneratedSource_ShouldCopyObsoleteAttribute()
    {
        // lang=C#-test
        var source = """
namespace ObsoleteMsgOnlyTest;

public class TestClass
{
    [System.Obsolete("Deprecated")]
    public event System.EventHandler MyEvent;
}

[R3Events.R3Event(typeof(TestClass))]
public static partial class TestExtensions
{
}
""";

        var generatedSources = CSharpGeneratorRunner.RunGeneratorAndGetGeneratedSources(source);

        generatedSources.ShouldNotBeEmpty("Generator should produce source for obsolete events as well as non-obsolete events");
        var extensionSource = generatedSources.Single(s => s.Contains("MyEventAsObservable"));
        extensionSource.ShouldContain("[global::System.Obsolete(\"Deprecated\")]");
    }
}
