using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using R3EventsGenerator.Utilities;

namespace R3EventsGenerator;

/// <summary>
/// Provides a code fix for <c>R3W001</c> that replaces the non-generic
/// <c>[R3Event(typeof(T))]</c> attribute with the generic <c>[R3Event&lt;T&gt;]</c> form,
/// which is available in C# 11 and later.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PreferGenericAttributeCodeFix))]
[Shared]
public sealed class PreferGenericAttributeCodeFix : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.PreferGenericAttribute.Id);

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var span = diagnostic.Location.SourceSpan;

        // Locate the AttributeSyntax node that spans the diagnostic location
        var attributeNode = root.FindNode(span) as AttributeSyntax;
        if (attributeNode is null)
        {
            return;
        }

        // The non-generic attribute must have exactly one argument: typeof(T)
        var argument = attributeNode.ArgumentList?.Arguments.FirstOrDefault();
        if (argument?.Expression is not TypeOfExpressionSyntax typeOfExpression)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use R3EventAttribute<T> instead",
                createChangedDocument: ct => ReplaceWithGenericAttributeAsync(
                    context.Document, root, attributeNode, typeOfExpression.Type, ct),
                equivalenceKey: "R3W001_UseGenericAttribute"
            ),
            diagnostic
        );
    }

    /// <summary>
    /// Rewrites the attribute node from <c>[R3Event(typeof(T))]</c> to <c>[R3Event&lt;T&gt;]</c>.
    /// The namespace qualification of the attribute name (if any) is preserved.
    /// </summary>
    private static Task<Document> ReplaceWithGenericAttributeAsync(
        Document document,
        SyntaxNode root,
        AttributeSyntax attributeNode,
        TypeSyntax typeArgument,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Build the generic name node — R3Event<T>
        // Strip leading/trailing trivia from the type argument to produce clean syntax
        var typeArgumentClean = typeArgument
            .WithLeadingTrivia(SyntaxTriviaList.Empty)
            .WithTrailingTrivia(SyntaxTriviaList.Empty);

        var genericName = SyntaxFactory.GenericName(
            SyntaxFactory.Identifier(GetSimpleName(attributeNode.Name).Identifier.Text),
            SyntaxFactory.TypeArgumentList(
                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(typeArgumentClean)));

        // Reconstruct the full attribute name, preserving any namespace qualification
        NameSyntax newName = attributeNode.Name switch
        {
            QualifiedNameSyntax qualified => qualified.WithRight(genericName),
            AliasQualifiedNameSyntax aliasQualified => aliasQualified.WithName(genericName),
            _ => genericName
        };

        // Replace the original attribute: new generic name, no constructor arguments
        var newAttribute = attributeNode
            .WithName(newName)
            .WithArgumentList(null)
            .WithTriviaFrom(attributeNode);

        var newRoot = root.ReplaceNode(attributeNode, newAttribute);
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }

    /// <summary>
    /// Returns the rightmost <see cref="SimpleNameSyntax"/> from a potentially qualified name.
    /// For example, <c>R3Events.R3Event</c> returns the <c>R3Event</c> identifier node.
    /// </summary>
    private static SimpleNameSyntax GetSimpleName(NameSyntax name) =>
        name switch
        {
            QualifiedNameSyntax qualified => qualified.Right,
            AliasQualifiedNameSyntax aliasQualified => aliasQualified.Name,
            SimpleNameSyntax simple => simple,
            _ => throw new InvalidOperationException($"Unexpected name syntax kind: {name.Kind()}")
        };
}
