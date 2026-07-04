using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using R3EventsGenerator.Utilities;

namespace R3EventsGenerator;

partial class R3EventsGenerator
{
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

    /// <summary>
    /// Reports any declaration-constraint violations for a class decorated with the generic R3EventAttribute.
    /// </summary>
    /// <param name="spc">The source production context used to report diagnostics.</param>
    /// <param name="item">The parsed diagnostic property to inspect.</param>
    private static void EmitDiagnosticsOutput(SourceProductionContext spc, ParsedDiagnosticProperty item)
    {
        if (Diagnose(item) is { } diag)
        {
            spc.ReportDiagnostic(diag);
        }
    }

    /// <summary>
    /// Emits the generated source file for a class decorated with an R3EventAttribute when the declaration is valid.
    /// </summary>
    /// <param name="spc">The source production context used to add the generated source.</param>
    /// <param name="item">The parsed generation property containing event information and target type details.</param>
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

    /// <summary>
    /// Returns <see langword="true"/> when the attributed class violates a declaration constraint
    /// and the source generator should skip code emission.
    /// </summary>
    /// <param name="item">The parsed generation property to inspect.</param>
    /// <returns>
    /// <see langword="true"/> if the class is nested, non-static, generic, or non-partial;
    /// otherwise <see langword="false"/>.
    /// </returns>
    private static bool HasDeclarationError(ParsedGenerationProperty item)
    {
        return item.IsNested || !item.IsStatic || item.IsGeneric || !item.IsPartial;
    }
}
