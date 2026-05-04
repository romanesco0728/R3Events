---
description: 'Guidelines specific to the Roslyn incremental source generator implementation in R3EventsGenerator.'
applyTo: 'src/main/R3EventsGenerator/**/*.cs'
---

# Source Generator Implementation Guidelines

## Roslyn Incremental Generator Patterns

- Implement `IIncrementalGenerator` (not the legacy `ISourceGenerator`) for performance.
- Keep the `Initialize` pipeline pure and allocation-free; push work to the transform and output stages.
- Use `SyntaxValueProvider.ForAttributeWithMetadataName` to filter syntax nodes by attribute without manual SyntaxReceiver logic.
- Cache expensive semantic model lookups with `IncrementalValueProvider` combinators (`Collect`, `Combine`, `Select`).
- Make all pipeline steps idempotent: avoid side effects and mutable state captured in lambdas.

## Code Emission Rules

- Every type referenced in emitted code **must** use the `global::` fully-qualified name.
  - Correct: `global::R3.Observable<global::System.String>`
  - Incorrect: `Observable<string>` (depends on consumer's `using` directives)
- Emitted files must use **block-style namespace declarations** (`namespace X.Y { ... }`), not file-scoped namespaces.
- Emitted partial type declarations must **not** include an access modifier — let the hand-authored declaration control accessibility.
- Use `$$"""..."""` interpolated raw string literals for templates that embed generator variables; use `"""..."""` for static templates.

## Diagnostics

- Report actionable diagnostics via `SourceProductionContext.ReportDiagnostic` for any user error (e.g., target type not found, attribute misuse).
- Use `DiagnosticSeverity.Error` for errors that prevent generation; `Warning` for recoverable issues.
- Define diagnostic descriptors as `static readonly` fields on a dedicated `Diagnostics` class.

## Testing

- Test generator output with `Microsoft.CodeAnalysis.CSharp.Testing` or snapshot-based tests.
- Verify that emitted code compiles without errors in a fresh compilation (no consumer `using` directives present).
- Test edge cases: events with custom delegates, generic target types, no public events.
