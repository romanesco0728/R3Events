using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace IncrementalSourceGeneratorStudy;

[Generator(LanguageNames.CSharp)]
public partial class SampleGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
        {
            var code = @"namespace Events.R3
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    internal sealed class R3EventAttribute : Attribute
    {
        public R3EventAttribute() { }

        public string? Name { get; set; }
    }
}"
            ;

            ctx.AddSource("Events.R3.R3EventAttribute.g.cs", SourceText.From(code, Encoding.UTF8));
        });
    }
}
