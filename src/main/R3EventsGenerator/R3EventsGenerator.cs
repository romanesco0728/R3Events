using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace R3EventsGenerator;

/// <summary>
/// Implements a source generator that produces Observable extension methods for classes decorated with the
/// R3EventAttribute, enabling reactive event handling in C# projects.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class R3EventsGenerator : IIncrementalGenerator
{
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
                transform: static (ctx, cancellationToken) => R3EventsGeneratorParsing.Parse(ctx, cancellationToken)
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
                transform: static (ctx, cancellationToken) => R3EventsGeneratorParsing.ParseDiagnostic(ctx, cancellationToken)
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
                transform: static (ctx, cancellationToken) => R3EventsGeneratorParsing.ParseGeneric(ctx, cancellationToken)
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
                transform: static (ctx, cancellationToken) => R3EventsGeneratorParsing.ParseGenericDiagnostic(ctx, cancellationToken)
                )
            .WithTrackingName("R3Events.GenericDiag.0_CreateSyntaxProvider");

        // Generate source output for each attributed class (non-generic)
        context.RegisterSourceOutput(source, static (spc, item) => R3EventsGeneratorEmission.EmitSourceOutput(spc, item));
        // Report diagnostics/warnings for each attributed class (non-generic), with language version for warning
        var sourceDiagnosticsWithLangVersion = sourceDiagnostics.Combine(languageVersionProvider);
        context.RegisterSourceOutput(sourceDiagnosticsWithLangVersion, static (spc, pair) => R3EventsGeneratorEmission.EmitNonGenericDiagnosticsOutput(spc, pair.Left, pair.Right));

        // Generate source output for each attributed class (generic)
        context.RegisterSourceOutput(genericSource, static (spc, item) => R3EventsGeneratorEmission.EmitSourceOutput(spc, item));
        // Report diagnostics for each attributed class (generic)
        context.RegisterSourceOutput(genericSourceDiagnostics, static (spc, item) => R3EventsGeneratorEmission.EmitDiagnosticsOutput(spc, item));
    }
}