using Microsoft.CodeAnalysis;

namespace IncrementalSourceGeneratorStudy;

[Generator(LanguageNames.CSharp)]
public partial class SampleGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {

    }
}
