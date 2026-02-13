using EventsR3Generator.Tests.Utilities;
using Shouldly;

namespace EventsR3Generator.Tests;

[TestClass]
public sealed class ErrorTests
{
    [TestMethod]
    public void R3EventOnNonStaticClass_ShouldProduceER003Error()
    {
        // lang=C#-test
        var source = """
namespace ErrorTest;

[Events.R3.R3Event(typeof(int))]
public partial class IntExtensions
{
}
""";

        var result = CSharpGeneratorRunner.RunGenerator(source);

        result.Length.ShouldBe(1, "Generator should produce exactly one diagnostic");
        result[0].Id.ShouldBe("ER003", "Diagnostic ID should be ER003 for non-static class error");
    }
}
