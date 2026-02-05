using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace IncrementalSourceGeneratorStudy;

[Generator(LanguageNames.CSharp)]
public partial class SampleGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Emit the R3EventAttribute definition in post-initialization
        context.RegisterPostInitializationOutput(EmitDefaultAttribute);

        // Use ForAttributeWithMetadataName to efficiently find classes decorated with R3EventAttribute
        var attributedClasses = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "Events.R3.R3EventAttribute",
                predicate: static (node, cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
                },
                transform: static (ctx, cancellationToken) => Parse(ctx, cancellationToken)
                );

        // Generate source output for each attributed class
        context.RegisterSourceOutput(attributedClasses, static (spc, item) =>
        {
            var (classSymbol, targetType) = item;

                //var generatedNamespace = "Events.R3.Generated";
                // Build methods using interpolated verbatim strings for compatibility
                var methodsBuilder = new StringBuilder();

                foreach (var member in targetType.GetMembers())
                {
                    if (member is not IEventSymbol ev || ev.DeclaredAccessibility != Accessibility.Public)
                        continue;

                    var eventType = ev.Type as INamedTypeSymbol;
                    ITypeSymbol? payloadType = null;

                    if (eventType != null && !eventType.IsGenericType && eventType.ToDisplayString() == "System.EventHandler")
                    {
                        // unit
                    }
                    else if (eventType != null && eventType.IsGenericType && eventType.ConstructedFrom?.ToDisplayString() == "System.EventHandler<TEventArgs>")
                    {
                        payloadType = eventType.TypeArguments[0];
                    }
                    else
                    {
                        var invoke = eventType?.DelegateInvokeMethod;
                        if (invoke != null)
                        {
                            var ps = invoke.Parameters;
                            if (ps.Length >= 1)
                                payloadType = ps[ps.Length - 1].Type;
                        }
                    }

                    if (payloadType is null && !(eventType is not null && !eventType.IsGenericType && eventType.ToDisplayString() == "System.EventHandler"))
                    {
                        // Default to System.Object since compilation reference is not available
                        payloadType = null;
                    }

                    string observableElementType;
                    bool useAsUnit = false;
                    if (eventType is not null && !eventType.IsGenericType && eventType.ToDisplayString() == "System.EventHandler")
                    {
                        observableElementType = "global::R3.Unit";
                        useAsUnit = true;
                    }
                    else if (payloadType is not null)
                    {
                        var payloadDisplay = payloadType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        observableElementType = payloadDisplay;
                    }
                    else
                    {
                        observableElementType = "global::System.Object";
                    }

                    var eventName = ev.Name;
                    var targetTypeDisplay = targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                    if (useAsUnit)
                    {
                        var method = $$"""
                                /// <summary>
                                /// Returns an Observable for <c>{{eventName}}</c>.
                                /// </summary>
                                public static global::R3.Observable<{{observableElementType}}> {{eventName}}AsObservable(this {{targetTypeDisplay}} instance, global::System.Threading.CancellationToken cancellationToken = default)
                                {
                                    var rawObservable = global::R3.Observable.FromEventHandler(
                                        h => instance.{{eventName}} += h,
                                        h => instance.{{eventName}} -= h,
                                        cancellationToken
                                        );
                                    return rawObservable.AsUnitObservable();
                                }
""";
                        methodsBuilder.AppendLine(method);
                    }
                    else
                    {
                        var delegateTypeDisplay = eventType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "global::System.Delegate";
                        var payloadTypeDisplay = observableElementType;
                        var tupleType = $"(object?, {payloadTypeDisplay} Args)";

                        var method = $$"""
        /// <summary>
        /// Returns an Observable for <c>{{eventName}}</c>.
        /// </summary>
        public static global::R3.Observable<{{observableElementType}}> {{eventName}}AsObservable(this {{targetTypeDisplay}} instance, global::System.Threading.CancellationToken cancellationToken = default)
        {
            var rawObservable = global::R3.Observable.FromEvent<{{delegateTypeDisplay}}, {{tupleType}}>(
                static h => new {{delegateTypeDisplay}}((s, e) => h((s, e))),
                h => instance.{{eventName}} += h,
                h => instance.{{eventName}} -= h,
                cancellationToken
                );
            return global::R3.ObservableExtensions.Select(rawObservable, ep => ep.Args);
        }
""";
                        methodsBuilder.AppendLine(method);
                    }
                }

                // Namespace of the attribute-bearing class. Empty for global namespace.
                var classNamespace = classSymbol.ContainingNamespace != null && !classSymbol.ContainingNamespace.IsGlobalNamespace
                    ? classSymbol.ContainingNamespace.ToDisplayString()
                    : string.Empty;

                string sourceText;
                if (classNamespace.Length > 0)
                {
                    sourceText = $$"""
// <auto-generated />
namespace {{classNamespace}}
{
    static partial class {{classSymbol.Name}}
    {
{{methodsBuilder}}
    }
}
""";
                }
                else
                {
                    sourceText = $$"""
// <auto-generated />
static partial class {{classSymbol.Name}}
{
{{methodsBuilder}}
}
""";
                }

                // File name: <namespace>.<ClassName>.g.cs (or <ClassName>.g.cs for global namespace)
                var fileName = string.IsNullOrEmpty(classNamespace)
                    ? $"{classSymbol.Name}.g.cs"
                    : $"{classNamespace}.{classSymbol.Name}.g.cs";

                spc.AddSource(fileName, SourceText.From(sourceText, Encoding.UTF8));
        });
    }

    private static void EmitDefaultAttribute(IncrementalGeneratorPostInitializationContext context)
    {
        var code = """
// <auto-generated />
namespace Events.R3
{
    [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    internal sealed class R3EventAttribute : global::System.Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="global::Events.R3.R3EventAttribute"/> class with the specified target type.
        /// </summary>
        /// <param name="type">The target <see cref="global::System.Type"/> the attribute refers to.</param>
        public R3EventAttribute(global::System.Type type)
        {
            this.Type = type ?? throw new global::System.ArgumentNullException(nameof(type));
        }

        /// <summary>
        /// Gets the target <see cref="global::System.Type"/> represented by this attribute.
        /// </summary>
        public global::System.Type Type { get; }
    }
}
""";
        context.AddSource("Events.R3.R3EventAttribute.g.cs", SourceText.From(code, Encoding.UTF8));
    }

    private static ParsedProperty Parse(GeneratorAttributeSyntaxContext ctx, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var classSymbol = (INamedTypeSymbol)ctx.TargetSymbol;
        var attrib = ctx.Attributes[0];
        var arg = attrib.ConstructorArguments[0];
        var targetTypeSymbol = (INamedTypeSymbol)arg.Value!;
        return new(ClassSymbol: classSymbol, TargetType: targetTypeSymbol);
    }

    private sealed record ParsedProperty(INamedTypeSymbol ClassSymbol, INamedTypeSymbol TargetType);
}
