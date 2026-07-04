using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace R3EventsGenerator;

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