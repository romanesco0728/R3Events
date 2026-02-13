using EventsR3Generator.Tests.Utilities;

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

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.AreEqual("ER003", result[0].Id);
    }
}
