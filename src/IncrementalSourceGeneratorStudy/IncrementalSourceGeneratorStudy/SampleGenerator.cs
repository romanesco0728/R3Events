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
        context.RegisterPostInitializationOutput(EmitDefaultAttribute);

        // Find class declarations that have attributes (candidate for the R3EventAttribute)
        var classWithAttributes = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (s, _) => s is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0,
            transform: static (ctx, _) =>
            {
                var classDecl = (ClassDeclarationSyntax)ctx.Node;
                var classSymbol = ctx.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
                if (classSymbol == null)
                    return default((INamedTypeSymbol? ClassSymbol, INamedTypeSymbol? TargetType));

                // Look for Events.R3.R3EventAttribute on the class
                foreach (var attrib in classSymbol.GetAttributes())
                {
                    var attrClass = attrib.AttributeClass;
                    if (attrClass == null)
                        continue;

                    // match by metadata name to be robust even when attribute is generated in post-init
                    if (attrClass.ToDisplayString() == "Events.R3.R3EventAttribute")
                    {
                        // Expect one constructor argument: typeof(TargetType)
                        if (attrib.ConstructorArguments.Length == 1)
                        {
                            var arg = attrib.ConstructorArguments[0];
                            if (arg.Value is INamedTypeSymbol targetTypeSymbol)
                            {
                                return (ClassSymbol: classSymbol, TargetType: targetTypeSymbol);
                            }
                        }
                    }
                }

                return default((INamedTypeSymbol? ClassSymbol, INamedTypeSymbol? TargetType));
            })
            .Where(static m => m.ClassSymbol is not null && m.TargetType is not null);

        // Collect and generate source
        var collected = classWithAttributes.Collect();
        var compilationAndItems = context.CompilationProvider.Combine(collected);
        context.RegisterSourceOutput(compilationAndItems, static (spc, pair) =>
        {
            var compilation = pair.Left;
            var items = pair.Right; // ImmutableArray<(INamedTypeSymbol? ClassSymbol, INamedTypeSymbol? TargetType)>
            foreach (var item in items)
            {
                var classSymbol = item.ClassSymbol;
                var targetType = item.TargetType;
                if (classSymbol == null || targetType == null)
                    continue;

                var generatedNamespace = "Events.R3.Generated";
                // Build methods using interpolated verbatim strings for compatibility
                var methodsBuilder = new StringBuilder();

                // Determine if System.ComponentModel is needed (e.g. CancelEventArgs)
                bool needsComponentModel = false;

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

                    if (payloadType == null && !(eventType != null && !eventType.IsGenericType && eventType.ToDisplayString() == "System.EventHandler"))
                    {
                        payloadType = compilation.GetTypeByMetadataName("System.Object");
                    }

                    string observableElementType;
                    bool useAsUnit = false;
                    if (eventType != null && !eventType.IsGenericType && eventType.ToDisplayString() == "System.EventHandler")
                    {
                        observableElementType = "global::R3.Unit";
                        useAsUnit = true;
                    }
                    else if (payloadType != null)
                    {
                        var payloadDisplay = payloadType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        observableElementType = payloadDisplay;
                        if (payloadDisplay.StartsWith("global::System.ComponentModel."))
                            needsComponentModel = true;
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
                        methodsBuilder.Append(method);
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
                        methodsBuilder.Append(method);
                    }
                }

                var usingComponent = needsComponentModel ? "    using global::System.ComponentModel;\n" : string.Empty;
                var classAccessibility = classSymbol.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";

                var header = @$"// <auto-generated />
namespace {generatedNamespace}
{{
    using global::System;
    using global::System.Threading;
{usingComponent}    {classAccessibility} static partial class {classSymbol.Name}
    {{
";

                var footer = @"
    }
}
";

                var sourceText = header + methodsBuilder.ToString() + footer;

                var fileName = $"Events.R3.Generated.{targetType.Name}.g.cs";
                spc.AddSource(fileName, SourceText.From(sourceText, Encoding.UTF8));
            }
        });
    }

    private static void EmitDefaultAttribute(IncrementalGeneratorPostInitializationContext context)
    {
        var code = """
            namespace Events.R3
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class | global::System.AttributeTargets.Struct | global::System.AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
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
}
