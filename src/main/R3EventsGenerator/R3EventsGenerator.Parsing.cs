using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace R3EventsGenerator;

/// <summary>
/// Provides parsing methods for extracting attribute and class information used in source generation.
/// </summary>
internal static class R3EventsGeneratorParsing
{
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
    internal static ParsedGenerationProperty Parse(GeneratorAttributeSyntaxContext ctx, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var classSymbol = (INamedTypeSymbol)ctx.TargetSymbol;
        var classDeclaration = (ClassDeclarationSyntax)ctx.TargetNode;
        var attrib = ctx.Attributes[0];
        var arg = attrib.ConstructorArguments[0];
        var targetTypeSymbol = (INamedTypeSymbol)arg.Value!;
        var obsoleteAttributeType = ctx.SemanticModel.Compilation.GetTypeByMetadataName("System.ObsoleteAttribute");

        return BuildParsedGenerationProperty(classSymbol, classDeclaration, targetTypeSymbol, obsoleteAttributeType);
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
    internal static ParsedGenerationProperty ParseGeneric(GeneratorAttributeSyntaxContext ctx, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var classSymbol = (INamedTypeSymbol)ctx.TargetSymbol;
        var classDeclaration = (ClassDeclarationSyntax)ctx.TargetNode;
        var attrib = ctx.Attributes[0];

        // For generic attribute, the type is specified as a type argument
        var targetTypeSymbol = (INamedTypeSymbol)attrib.AttributeClass!.TypeArguments[0];
        var obsoleteAttributeType = ctx.SemanticModel.Compilation.GetTypeByMetadataName("System.ObsoleteAttribute");

        return BuildParsedGenerationProperty(classSymbol, classDeclaration, targetTypeSymbol, obsoleteAttributeType);
    }

    /// <summary>
    /// Parses the provided generator attribute context (non-generic variant) to extract diagnostic location information.
    /// </summary>
    /// <remarks>This method throws an <see cref="OperationCanceledException"/> if the cancellation token is signaled.</remarks>
    /// <param name="ctx">The generator attribute syntax context containing the target symbol, node, and associated attributes.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the parsing operation.</param>
    /// <returns>A <see cref="ParsedDiagnosticProperty"/> instance containing class metadata and location information for diagnostics.</returns>
    internal static ParsedDiagnosticProperty ParseDiagnostic(GeneratorAttributeSyntaxContext ctx, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var classSymbol = (INamedTypeSymbol)ctx.TargetSymbol;
        var classDeclaration = (ClassDeclarationSyntax)ctx.TargetNode;
        var attrib = ctx.Attributes[0];
        var attributeLocation = attrib.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation() ?? Location.None;

        return BuildParsedDiagnosticProperty(classSymbol, classDeclaration, attributeLocation);
    }

    /// <summary>
    /// Parses the provided generator attribute context (generic variant) to extract diagnostic location information.
    /// </summary>
    /// <remarks>This method throws an <see cref="OperationCanceledException"/> if the cancellation token is signaled.</remarks>
    /// <param name="ctx">The generator attribute syntax context containing the target symbol, node, and associated attributes.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the parsing operation.</param>
    /// <returns>A <see cref="ParsedDiagnosticProperty"/> instance containing class metadata and location information for diagnostics.</returns>
    internal static ParsedDiagnosticProperty ParseGenericDiagnostic(GeneratorAttributeSyntaxContext ctx, CancellationToken cancellationToken)
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
    /// <param name="obsoleteAttributeType">The <see cref="System.ObsoleteAttribute"/> type symbol used for symbol-based attribute matching.</param>
    /// <returns>A parsed property instance used for source generation.</returns>
    private static ParsedGenerationProperty BuildParsedGenerationProperty(
        INamedTypeSymbol classSymbol,
        ClassDeclarationSyntax classDeclaration,
        INamedTypeSymbol targetTypeSymbol,
        INamedTypeSymbol? obsoleteAttributeType)
    {
        // Extract method information from the target type
        var generatedMethods = R3EventsGeneratorGeneration.ExtractGeneratedMethods(targetTypeSymbol, obsoleteAttributeType);
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

    /// <summary>
    /// Builds a diagnostic property model from class/attribute context shared by generic and non-generic attributes.
    /// </summary>
    /// <param name="classSymbol">The attributed class symbol.</param>
    /// <param name="classDeclaration">The attributed class declaration syntax.</param>
    /// <param name="attributeLocation">The source location of the attribute application site.</param>
    /// <returns>A parsed diagnostic property instance used for reporting diagnostics.</returns>
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
}
