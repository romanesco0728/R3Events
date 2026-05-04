using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using R3EventsGenerator.Utilities;

namespace R3EventsGenerator;

/// <summary>
/// Implements a source generator that produces Observable extension methods for classes decorated with the
/// R3EventAttribute, enabling reactive event handling in C# projects.
/// </summary>
[Generator(LanguageNames.CSharp)]
public partial class R3EventsGenerator : IIncrementalGenerator
{
    private static readonly SymbolDisplayFormat UserFacingTypeNameFormat = new(
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions:
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
            SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
    );

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get the compilation provider to access language version
        var compilationProvider = context.CompilationProvider;

        // Emit the generic R3EventAttribute<T> if language version supports it (C# 11+)
        var languageVersionProvider = compilationProvider.Select(static (compilation, _) =>
        {
            // Get the maximum language version across all syntax trees to ensure
            // the generic attribute is available when any file uses C# 11+
            var maxLanguageVersion = LanguageVersion.CSharp1;

            foreach (var tree in compilation.SyntaxTrees)
            {
                if (tree.Options is CSharpParseOptions parseOptions)
                {
                    if (parseOptions.LanguageVersion > maxLanguageVersion)
                    {
                        maxLanguageVersion = parseOptions.LanguageVersion;
                    }
                }
            }

            return maxLanguageVersion;
        });

        // Use ForAttributeWithMetadataName to efficiently find classes decorated with R3EventAttribute (non-generic)
        var source = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "R3Events.R3EventAttribute",
                predicate: static (node, cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
                },
                transform: static (ctx, cancellationToken) => Parse(ctx, cancellationToken)
                )
            .WithTrackingName("R3Events.NonGeneric.0_CreateSyntaxProvider");

        var sourceDiagnostics = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "R3Events.R3EventAttribute",
                predicate: static (node, cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
                },
                transform: static (ctx, cancellationToken) => ParseDiagnostic(ctx, cancellationToken)
                )
            .WithTrackingName("R3Events.NonGenericDiag.0_CreateSyntaxProvider");

        // Use ForAttributeWithMetadataName to efficiently find classes decorated with R3EventAttribute<T> (generic)
        var genericSource = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "R3Events.R3EventAttribute`1",
                predicate: static (node, cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
                },
                transform: static (ctx, cancellationToken) => ParseGeneric(ctx, cancellationToken)
                )
            .WithTrackingName("R3Events.Generic.0_CreateSyntaxProvider");

        var genericSourceDiagnostics = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "R3Events.R3EventAttribute`1",
                predicate: static (node, cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
                },
                transform: static (ctx, cancellationToken) => ParseGenericDiagnostic(ctx, cancellationToken)
                )
            .WithTrackingName("R3Events.GenericDiag.0_CreateSyntaxProvider");

        // Generate source output for each attributed class (non-generic)
        context.RegisterSourceOutput(source, static (spc, item) => EmitSourceOutput(spc, item));
        // Report diagnostics/warnings for each attributed class (non-generic), with language version for warning
        var sourceDiagnosticsWithLangVersion = sourceDiagnostics.Combine(languageVersionProvider);
        context.RegisterSourceOutput(sourceDiagnosticsWithLangVersion, static (spc, pair) => EmitNonGenericDiagnosticsOutput(spc, pair.Left, pair.Right));

        // Generate source output for each attributed class (generic)
        context.RegisterSourceOutput(genericSource, static (spc, item) => EmitSourceOutput(spc, item));
        // Report diagnostics for each attributed class (generic)
        context.RegisterSourceOutput(genericSourceDiagnostics, static (spc, item) => EmitDiagnosticsOutput(spc, item));
    }

    /// <summary>
    /// Parses the provided generator attribute context to extract property and method information for code generation.
    /// </summary>
    /// <remarks>
    /// This method throws an <see cref="OperationCanceledException"/> if the cancellation token is signaled.
    /// The returned <see cref="ParsedGenerationProperty"/> includes fully qualified names and method details for use in code generation scenarios.
    /// </remarks>
    /// <param name="ctx">The generator attribute context containing the target symbol, node, and associated attributes to be parsed.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the parsing operation.</param>
    /// <returns>
    /// A <see cref="ParsedGenerationProperty"/> instance containing extracted class metadata and generated method information based on the target type.
    /// </returns>
    private static ParsedGenerationProperty Parse(GeneratorAttributeSyntaxContext ctx, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var classSymbol = (INamedTypeSymbol)ctx.TargetSymbol;
        var classDeclaration = (ClassDeclarationSyntax)ctx.TargetNode;
        var attrib = ctx.Attributes[0];
        var arg = attrib.ConstructorArguments[0];
        var targetTypeSymbol = (INamedTypeSymbol)arg.Value!;

        return BuildParsedGenerationProperty(classSymbol, classDeclaration, targetTypeSymbol);
    }

    /// <summary>
    /// Parses the provided generator attribute context for the generic R3EventAttribute{T} to extract property and method information for code generation.
    /// </summary>
    /// <remarks>
    /// This method handles the generic attribute variant where the target type is specified as a type parameter.
    /// This method throws an <see cref="OperationCanceledException"/> if the cancellation token is signaled.
    /// The returned <see cref="ParsedGenerationProperty"/> includes fully qualified names and method details for use in code generation scenarios.
    /// </remarks>
    /// <param name="ctx">The generator attribute context containing the target symbol, node, and associated attributes to be parsed.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the parsing operation.</param>
    /// <returns>
    /// A <see cref="ParsedGenerationProperty"/> instance containing extracted class metadata and generated method information based on the target type.
    /// </returns>
    private static ParsedGenerationProperty ParseGeneric(GeneratorAttributeSyntaxContext ctx, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var classSymbol = (INamedTypeSymbol)ctx.TargetSymbol;
        var classDeclaration = (ClassDeclarationSyntax)ctx.TargetNode;
        var attrib = ctx.Attributes[0];

        // For generic attribute, the type is specified as a type argument
        var targetTypeSymbol = (INamedTypeSymbol)attrib.AttributeClass!.TypeArguments[0];

        return BuildParsedGenerationProperty(classSymbol, classDeclaration, targetTypeSymbol);
    }

    private static ParsedDiagnosticProperty ParseDiagnostic(GeneratorAttributeSyntaxContext ctx, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var classSymbol = (INamedTypeSymbol)ctx.TargetSymbol;
        var classDeclaration = (ClassDeclarationSyntax)ctx.TargetNode;
        var attrib = ctx.Attributes[0];
        var attributeLocation = attrib.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation() ?? Location.None;

        return BuildParsedDiagnosticProperty(classSymbol, classDeclaration, attributeLocation);
    }

    private static ParsedDiagnosticProperty ParseGenericDiagnostic(GeneratorAttributeSyntaxContext ctx, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var classSymbol = (INamedTypeSymbol)ctx.TargetSymbol;
        var classDeclaration = (ClassDeclarationSyntax)ctx.TargetNode;
        var attrib = ctx.Attributes[0];
        var attributeLocation = attrib.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation() ?? Location.None;

        return BuildParsedDiagnosticProperty(classSymbol, classDeclaration, attributeLocation);
    }

    /// <summary>
    /// Builds a parsed property model from class/attribute context shared by generic and non-generic attributes.
    /// </summary>
    /// <param name="classSymbol">The attributed class symbol.</param>
    /// <param name="classDeclaration">The attributed class declaration syntax.</param>
    /// <param name="targetTypeSymbol">The target type symbol referenced by the attribute.</param>
    /// <returns>A parsed property instance used for source generation.</returns>
    private static ParsedGenerationProperty BuildParsedGenerationProperty(
        INamedTypeSymbol classSymbol,
        ClassDeclarationSyntax classDeclaration,
        INamedTypeSymbol targetTypeSymbol)
    {
        // Extract method information from the target type
        var generatedMethods = ExtractGeneratedMethods(targetTypeSymbol);
        var targetTypeName = TypeNameView.FromTypeSymbol(targetTypeSymbol);
        var classNameView = TypeNameView.FromNamedTypeSymbol(classSymbol);

        // Get class namespace and name
        var containingNamespace = classSymbol.ContainingNamespace;
        var classNamespace = containingNamespace.IsGlobalNamespace ? string.Empty : containingNamespace.ToDisplayString();
        var className = classSymbol.Name;
        var isNested = classDeclaration.Parent is TypeDeclarationSyntax;
        var isStatic = classDeclaration.Modifiers.Any(static m => m.IsKind(SyntaxKind.StaticKeyword));
        var isGeneric = classDeclaration.TypeParameterList is not null;
        var isPartial = classDeclaration.Modifiers.Any(static m => m.IsKind(SyntaxKind.PartialKeyword));

        return new()
        {
            ClassNamespace = classNamespace,
            ClassName = className,
            ClassNameView = classNameView,
            GeneratedMethods = generatedMethods,
            TargetTypeName = targetTypeName,
            IsNested = isNested,
            IsStatic = isStatic,
            IsGeneric = isGeneric,
            IsPartial = isPartial,
        };
    }

    private static ParsedDiagnosticProperty BuildParsedDiagnosticProperty(
        INamedTypeSymbol classSymbol,
        ClassDeclarationSyntax classDeclaration,
        Location attributeLocation)
    {
        var partialLocation = classDeclaration.Identifier.GetLocation();
        return new()
        {
            ClassNameView = TypeNameView.FromNamedTypeSymbol(classSymbol),
            IsNested = classDeclaration.Parent is TypeDeclarationSyntax,
            IsStatic = classDeclaration.Modifiers.Any(static m => m.IsKind(SyntaxKind.StaticKeyword)),
            IsGeneric = classDeclaration.TypeParameterList is not null,
            IsPartial = classDeclaration.Modifiers.Any(static m => m.IsKind(SyntaxKind.PartialKeyword)),
            PartialLocation = new(partialLocation),
            PartialLocationKey = LocationKey.From(partialLocation),
            AttributeLocation = new(attributeLocation),
            AttributeLocationKey = LocationKey.From(attributeLocation),
        };
    }

    /// <summary>
    /// Extracts information about all public, non-static events declared in the specified type.
    /// </summary>
    /// <param name="targetType">
    /// The type symbol representing the target type from which to extract event method information.
    /// </param>
    /// <returns>
    /// An array containing information about each public, non-static event declared in the target type.
    /// The array is ordered by event name and will be empty if no such events are found.
    /// </returns>
    private static EquatableArray<GeneratedMethodInfo> ExtractGeneratedMethods(INamedTypeSymbol targetType)
    {
        var methodInfos = targetType.GetMembers()
            .OfType<IEventSymbol>()
            .Where(static ev => ev is { DeclaredAccessibility: Accessibility.Public, IsStatic: false })
            .Select(static x => GenerateMethodInfo(x))
            .OrderBy(static x => x.EventName)
            .ToArray();
        return new(methodInfos);
    }

    private static GeneratedMethodInfo GenerateMethodInfo(IEventSymbol ev)
    {
        var eventType = ev.Type as INamedTypeSymbol;
        ITypeSymbol? payloadType = null;

        var isNonGenericSystemEventHandler = eventType is { IsGenericType: false } &&
            eventType.ContainingNamespace?.Name is "System" &&
            eventType.MetadataName is "EventHandler";
        if (isNonGenericSystemEventHandler)
        {
            // unit
        }
        else
        {
            var invoke = eventType?.DelegateInvokeMethod;
            if (invoke != null)
            {
                var ps = invoke.Parameters;
                if (ps.Length >= 1) payloadType = ps[^1].Type;
            }
        }

        if (payloadType is null && !isNonGenericSystemEventHandler)
        {
            // Default to System.Object since compilation reference is not available
            payloadType = null;
        }

        TypeNameView observableElementType;
        bool useAsUnit = false;
        if (isNonGenericSystemEventHandler)
        {
            observableElementType = TypeNameView.Create("global::R3.Unit", "R3.Unit");
            useAsUnit = true;
        }
        else if (payloadType is not null)
        {
            observableElementType = TypeNameView.FromTypeSymbol(payloadType);
        }
        else
        {
            observableElementType = TypeNameView.Create("global::System.Object", "object");
        }

        var delegateType = eventType is not null
            ? TypeNameView.FromNamedTypeSymbol(eventType)
            : TypeNameView.Create("global::System.Delegate", "System.Delegate");

        return new()
        {
            EventName = ev.Name,
            ObservableElementType = observableElementType,
            UseAsUnit = useAsUnit,
            DelegateType = delegateType,
        };
    }

    /// <summary>
    /// Generates source output for a class decorated with the non-generic R3EventAttribute, and emits a
    /// <c>R3I001</c> info when the language version supports the generic attribute (C# 11 or later).
    /// </summary>
    /// <param name="spc">The source production context used to add generated source and report diagnostics.</param>
    /// <param name="item">The parsed property containing event information and target type details.</param>
    /// <param name="languageVersion">The C# language version in use, used to determine whether to suggest the generic attribute.</param>
    private static void EmitNonGenericDiagnosticsOutput(SourceProductionContext spc, ParsedDiagnosticProperty item, LanguageVersion languageVersion)
    {
        if (Diagnose(item) is { } diag)
        {
            spc.ReportDiagnostic(diag);
        }

        // Emit a warning when the non-generic attribute is used but C# 11+ makes the generic version available
        if (languageVersion >= LanguageVersion.CSharp11)
        {
            spc.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.PreferGenericAttribute,
                item.AttributeLocation,
                item.ClassNameView.UserFacing
            ));
        }
    }

    private static void EmitDiagnosticsOutput(SourceProductionContext spc, ParsedDiagnosticProperty item)
    {
        if (Diagnose(item) is { } diag)
        {
            spc.ReportDiagnostic(diag);
            return;
        }
    }

    private static void EmitSourceOutput(SourceProductionContext spc, ParsedGenerationProperty item)
    {
        if (HasDeclarationError(item))
        {
            return;
        }

        var hintName = $"{item.HintBaseName}.g.cs";
        spc.AddSource(hintName, SourceText.From(GenerateSource(item), Encoding.UTF8));
    }

    /// <summary>
    /// Analyzes the specified property and returns a diagnostic result.
    /// </summary>
    /// <param name="item">The property to analyze for class declaration requirements.</param>
    /// <returns>A diagnostic indicating declaration constraints if violated; otherwise, <see langword="null"/>.</returns>
    private static Diagnostic? Diagnose(ParsedDiagnosticProperty item)
    {
        if (item.IsNested)
        {
            return Diagnostic.Create(
                DiagnosticDescriptors.MustNotBeNested,
                item.PartialLocation,
                item.ClassNameView.UserFacing
                );
        }

        if (!item.IsStatic)
        {
            return Diagnostic.Create(
                DiagnosticDescriptors.MustBeStatic,
                item.PartialLocation,
                item.ClassNameView.UserFacing
                );
        }

        if (item.IsGeneric)
        {
            return Diagnostic.Create(
                DiagnosticDescriptors.MustNotBeGeneric,
                item.PartialLocation,
                item.ClassNameView.UserFacing
                );
        }

        if (!item.IsPartial)
        {
            return Diagnostic.Create(
                DiagnosticDescriptors.MustBePartial,
                item.PartialLocation,
                item.ClassNameView.UserFacing
                );
        }

        return null;
    }

    private static bool HasDeclarationError(ParsedGenerationProperty item)
    {
        return item.IsNested || !item.IsStatic || item.IsGeneric || !item.IsPartial;
    }

    /// <summary>
    /// Generates C# source code for extension methods that expose events of a parsed property as observables.
    /// </summary>
    /// <remarks>
    /// The generated source code includes extension methods for each event defined in the parsed property.
    /// These methods allow consumers to subscribe to events using the R3 observable pattern.
    /// The output is intended for use in code generation scenarios and is marked as auto-generated.
    /// </remarks>
    /// <param name="item">The parsed property containing event information and target type details used to generate observable extension  methods.
    /// </param>
    /// <returns>
    /// A string containing the generated C# source code for observable extension methods, including namespace and class declarations as appropriate.
    /// </returns>
    private static string GenerateSource(ParsedGenerationProperty item)
    {
        var methodsBuilder = new StringBuilder();

        foreach (var methodInfo in item.GeneratedMethods)
        {
            if (methodInfo.UseAsUnit)
            {
                var method = $$"""
        /// <summary>
        /// Returns an <see cref="R3.Observable`1"/> for <c>{{methodInfo.EventName}}</c> with payload type <see cref="{{methodInfo.ObservableElementType.UserFacing}}"/>.
        /// </summary>
        public static global::R3.Observable<{{methodInfo.ObservableElementType.CodeQualified}}> {{methodInfo.EventName}}AsObservable(this {{item.TargetTypeName.CodeQualified}} instance, global::System.Threading.CancellationToken cancellationToken = default)
        {
            var rawObservable = global::R3.Observable.FromEventHandler(
                h => instance.{{methodInfo.EventName}} += h,
                h => instance.{{methodInfo.EventName}} -= h,
                cancellationToken
                );
            return global::R3.ObservableExtensions.AsUnitObservable(rawObservable);
        }
""";
                methodsBuilder.AppendLine(method);
            }
            else
            {
                var method = $$"""
        /// <summary>
        /// Returns an <see cref="R3.Observable`1"/> for <c>{{methodInfo.EventName}}</c> with payload type <see cref="{{methodInfo.ObservableElementType.UserFacing}}"/>.
        /// </summary>
        public static global::R3.Observable<{{methodInfo.ObservableElementType.CodeQualified}}> {{methodInfo.EventName}}AsObservable(this {{item.TargetTypeName.CodeQualified}} instance, global::System.Threading.CancellationToken cancellationToken = default)
        {
            var rawObservable = global::R3.Observable.FromEvent<{{methodInfo.DelegateType.CodeQualified}}, (global::System.Object?, {{methodInfo.ObservableElementType.CodeQualified}} Args)>(
                h => new {{methodInfo.DelegateType.CodeQualified}}((s, e) => h((s, e))),
                h => instance.{{methodInfo.EventName}} += h,
                h => instance.{{methodInfo.EventName}} -= h,
                cancellationToken
                );
            return global::R3.ObservableExtensions.Select(rawObservable, ep => ep.Args);
        }
""";
                methodsBuilder.AppendLine(method);
            }
        }

        // Namespace of the attribute-bearing class. Empty for global namespace.
        var classNamespace = item.ClassNamespace;
        var className = item.ClassName;

        if (classNamespace.Length > 0)
        {
            return $$"""
// <auto-generated />
#nullable enable
namespace {{classNamespace}}
{
    partial class {{className}}
    {
{{methodsBuilder}}
    }
}
""";
        }
        else
        {
            return $$"""
// <auto-generated />
#nullable enable
partial class {{className}}
{
{{methodsBuilder}}
}
""";
        }
    }


    /// <summary>
    /// Represents metadata describing a generated method for an observable event, including event name, element type,
    /// and delegate information.
    /// </summary>
    private sealed record GeneratedMethodInfo
    {
        /// <summary>
        /// Gets the name of the event associated with this instance.
        /// </summary>
        public required string EventName { get; init; }
        /// <summary>
        /// Gets the name of the element type that is being observed.
        /// </summary>
        public required TypeNameView ObservableElementType { get; init; }
        /// <summary>
        /// Gets a value indicating whether to use R3.Unit as the element type for the observable.
        /// </summary>
        public required bool UseAsUnit { get; init; }
        /// <summary>
        /// Gets the fully qualified name of the delegate type used for the event handler.
        /// </summary>
        public required TypeNameView DelegateType { get; init; }
    }

    /// <summary>
    /// Carries dual type-name representations for generated code and user-facing text.
    /// </summary>
    private sealed record TypeNameView
    {
        /// <summary>
        /// Gets the type name formatted for generated code emission with explicit <c>global::</c> qualification.
        /// </summary>
        public required string CodeQualified { get; init; }

        /// <summary>
        /// Gets the type name formatted for user-facing text such as diagnostics and XML comments.
        /// </summary>
        public required string UserFacing { get; init; }

        /// <summary>
        /// Creates a new type-name view from explicit code and user-facing values.
        /// </summary>
        /// <param name="codeQualified">The code-safe representation.</param>
        /// <param name="userFacing">The user-facing representation.</param>
        /// <returns>A populated type-name view.</returns>
        public static TypeNameView Create(string codeQualified, string userFacing)
        {
            return new()
            {
                CodeQualified = codeQualified,
                UserFacing = userFacing,
            };
        }

        /// <summary>
        /// Creates a type-name view from a type symbol.
        /// </summary>
        /// <param name="symbol">The source symbol.</param>
        /// <returns>The corresponding type-name view.</returns>
        public static TypeNameView FromTypeSymbol(ITypeSymbol symbol)
        {
            return Create(
                symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                symbol.ToDisplayString(UserFacingTypeNameFormat)
            );
        }

        /// <summary>
        /// Creates a type-name view from a named type symbol.
        /// </summary>
        /// <param name="symbol">The source symbol.</param>
        /// <returns>The corresponding type-name view.</returns>
        public static TypeNameView FromNamedTypeSymbol(INamedTypeSymbol symbol)
        {
            return Create(
                symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                symbol.ToDisplayString(UserFacingTypeNameFormat)
            );
        }
    }

    /// <summary>
    /// Represents a deterministic key for Roslyn locations used in incremental equality comparisons.
    /// </summary>
    private readonly record struct LocationKey(string FilePath, int Start, int Length, bool IsInSource)
    {
        /// <summary>
        /// Creates a stable key from the specified location.
        /// </summary>
        /// <param name="location">The location to convert into a key.</param>
        /// <returns>A key containing file path and span information.</returns>
        public static LocationKey From(Location location)
        {
            if (!location.IsInSource || location.SourceTree is null)
            {
                return new(string.Empty, 0, 0, false);
            }

            var span = location.SourceSpan;
            return new(location.SourceTree.FilePath, span.Start, span.Length, true);
        }
    }

    /// <summary>
    /// Represents metadata for a class and its associated generated Observable extension methods as parsed from an
    /// R3EventAttribute.
    /// </summary>
    private sealed record ParsedGenerationProperty
    {
        /// <summary>
        /// Gets the fully qualified namespace of the attributed class, or an empty string if the class is in the global namespace.
        /// </summary>
        public required string ClassNamespace { get; init; }
        /// <summary>
        /// Gets the name of the attributed class.
        /// </summary>
        public required string ClassName { get; init; }
        /// <summary>
        /// Gets a collection of metadata describing the generated Observable extension methods for the attributed class.
        /// </summary>
        public required EquatableArray<GeneratedMethodInfo> GeneratedMethods { get; init; }
        /// <summary>
        /// Gets the attributed class name model containing both code-qualified and user-facing representations.
        /// </summary>
        public required TypeNameView ClassNameView { get; init; }
        /// <summary>
        /// Gets the target type name model containing both code-qualified and user-facing representations.
        /// </summary>
        public required TypeNameView TargetTypeName { get; init; }
        /// <summary>
        /// Gets a value indicating whether the attributed class is nested within another type.
        /// </summary>
        public required bool IsNested { get; init; }
        /// <summary>
        /// Gets a value indicating whether the attributed class is declared as static.
        /// </summary>
        public required bool IsStatic { get; init; }
        /// <summary>
        /// Gets a value indicating whether the attributed class is a generic type.
        /// </summary>
        public required bool IsGeneric { get; init; }
        /// <summary>
        /// Gets a value indicating whether the attributed class is declared as partial.
        /// </summary>
        public required bool IsPartial { get; init; }
        /// <summary>
        /// Gets the base hint name used for generated source file names.
        /// </summary>
        public string HintBaseName => (string.IsNullOrEmpty(ClassNamespace) ? ClassName : $"{ClassNamespace}.{ClassName}")
            .Replace("global::", "")
            .Replace("<", "_")
            .Replace(">", "_");
    }

    private sealed record ParsedDiagnosticProperty
    {
        /// <summary>
        /// Gets the attributed class name model containing both code-qualified and user-facing representations.
        /// </summary>
        public required TypeNameView ClassNameView { get; init; }
        /// <summary>
        /// Gets the location of the R3EventAttribute application site (the attribute node in source).
        /// Used to position diagnostics and code-fix actions at the attribute rather than the class declaration.
        /// </summary>
        public required IgnoreEquality<Location> AttributeLocation { get; init; }
        /// <summary>
        /// Gets the deterministic comparison key for the attribute application location.
        /// </summary>
        public required LocationKey AttributeLocationKey { get; init; }
        /// <summary>
        /// Gets a value indicating whether the attributed class is nested within another type.
        /// </summary>
        public required bool IsNested { get; init; }
        /// <summary>
        /// Gets a value indicating whether the attributed class is declared as static.
        /// </summary>
        public required bool IsStatic { get; init; }
        /// <summary>
        /// Gets a value indicating whether the attributed class is a generic type.
        /// </summary>
        public required bool IsGeneric { get; init; }
        /// <summary>
        /// Gets a value indicating whether the attributed class is declared as partial.
        /// </summary>
        public required bool IsPartial { get; init; }
        /// <summary>
        /// Gets the location of the class identifier for diagnostics related to class declaration requirements.
        /// </summary>
        public required IgnoreEquality<Location> PartialLocation { get; init; }
        /// <summary>
        /// Gets the deterministic comparison key for the class identifier location.
        /// </summary>
        public required LocationKey PartialLocationKey { get; init; }
    }
}
