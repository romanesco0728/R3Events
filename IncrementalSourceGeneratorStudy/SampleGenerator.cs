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
    using global::System;

    [global::System.AttributeUsage(global::System.AttributeTargets.Class | global::System.AttributeTargets.Struct | global::System.AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    internal sealed class R3EventAttribute : global::System.Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref=""global::Events.R3.R3EventAttribute""/> class with the specified target type.
        /// </summary>
        /// <param name=""type"">The target <see cref=""global::System.Type""/> the attribute refers to.</param>
        public R3EventAttribute(global::System.Type type)
        {
            this.Type = type ?? throw new global::System.ArgumentNullException(nameof(type));
        }

        /// <summary>
        /// Gets the target <see cref=""global::System.Type""/> represented by this attribute.
        /// </summary>
        public global::System.Type Type { get; }
    }
}"
            ;

            ctx.AddSource("Events.R3.R3EventAttribute.g.cs", SourceText.From(code, Encoding.UTF8));
        });
    }
}
