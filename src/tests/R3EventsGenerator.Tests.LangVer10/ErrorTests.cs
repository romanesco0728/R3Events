using R3EventsGenerator.Tests.LangVer10.Utilities;
using Shouldly;

namespace R3EventsGenerator.Tests.LangVer10;

[TestClass]
public sealed class ErrorTests
{
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
        result[0].Id.ShouldBe("ER001", "Diagnostic ID should be ER001 for non-partial class error");
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
        result[0].Id.ShouldBe("ER002", "Diagnostic ID should be ER002 for nested class error");
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
        result[0].Id.ShouldBe("ER003", "Diagnostic ID should be ER003 for non-static class error");
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
        result[0].Id.ShouldBe("ER004", "Diagnostic ID should be ER004 for generic class error");
    }
}
