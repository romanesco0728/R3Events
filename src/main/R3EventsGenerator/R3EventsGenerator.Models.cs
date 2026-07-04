using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using R3EventsGenerator.Utilities;

namespace R3EventsGenerator;

/// <summary>
/// Represents metadata describing a generated method for an observable event, including event name, element type,
/// and delegate information.
/// </summary>
internal sealed record GeneratedMethodInfo
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
    /// <summary>
    /// Gets the obsolete metadata copied from the source event when present.
    /// </summary>
    public required GeneratedObsoleteInfo? ObsoleteInfo { get; init; }
}

/// <summary>
/// Represents obsolete metadata copied from the source event for method emission.
/// </summary>
internal sealed record GeneratedObsoleteInfo
{
    /// <summary>
    /// Gets the obsolete message supplied by the source event, if any.
    /// </summary>
    public required string? Message { get; init; }
    /// <summary>
    /// Gets a value indicating whether the obsolete message argument was explicitly supplied.
    /// </summary>
    public required bool HasMessageArgument { get; init; }
    /// <summary>
    /// Gets a value indicating whether the obsolete error argument was explicitly supplied.
    /// </summary>
    public required bool HasErrorArgument { get; init; }
    /// <summary>
    /// Gets a value indicating whether the obsolete attribute is an error.
    /// </summary>
    public required bool IsError { get; init; }
}

/// <summary>
/// Carries dual type-name representations for generated code and user-facing text.
/// </summary>
internal sealed record TypeNameView
{
    private static readonly SymbolDisplayFormat UserFacingTypeNameFormat = new(
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions:
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
            SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
    );

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
internal readonly record struct LocationKey(string FilePath, int Start, int Length, bool IsInSource)
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
internal sealed record ParsedGenerationProperty
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

internal sealed record ParsedDiagnosticProperty
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
