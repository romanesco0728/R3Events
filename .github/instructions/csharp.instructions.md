---
description: 'Guidelines for building C# applications'
applyTo: '**/*.cs'
---

# C# Development

## C# Instructions
- Always use the latest version C#, currently C# 14 features.
- Write clear and concise comments for each function.

## General Instructions
- Make only high confidence suggestions when reviewing code changes.
- Write code with good maintainability practices, including comments on why certain design decisions were made.
- Handle edge cases and write clear exception handling.
- For libraries or external dependencies, mention their usage and purpose in comments.

## Naming Conventions

- Follow PascalCase for component names, method names, and public members.
- Use camelCase for private fields and local variables.
- Prefix interface names with "I" (e.g., IUserService).

## Formatting

- Prefer file-scoped namespace declarations for regular (hand-authored) source files.

- For automatically generated source produced by source generators, use the traditional block-style namespace declaration (e.g., `namespace X.Y { ... }`). Generated files MUST use block-style namespaces to avoid edge cases when merging partial types and to maximize compatibility with various compiler and IDE behaviors.

- Using directives in generated files:
  - Generated files MUST NOT contain `using` directives. Generated code should reference types using their fully-qualified names starting with `global::`. This avoids ambiguity and ensures generated code is independent of the surrounding user code's `using` directives.
  - As a rule of thumb, do not emit any `using` statements in automatically generated files. If a generator must include `using` directives as a last resort for readability, those `using` directives should be placed inside the namespace block and indented once (4 spaces); however, prefer fully-qualified `global::` names for public API types and for any types that might collide.
  - Example of a generated file that does not use `using` directives:

    namespace MyApp.Generated
    {
        internal static partial class MyExtensions
        {
            public static global::R3.Observable<global::System.String> EventNameAsObservable(this global::MyApp.TargetType instance)
            {
                // ...generated members...
            }
        }
    }

- When generating additional partial declarations for an existing partial type:
  - Do not emit an accessibility modifier (e.g., `public`, `internal`, `private`, `protected`) on the generated partial type declaration. Leave the access level to be determined by the hand-authored declaration to avoid accidental accessibility mismatches.
  - Example:

    namespace MyApp.Generated
    {
        // Note: no 'public' or 'internal' on the generated partial type declaration
        static partial class Helpers
        {
            // generated members...
        }
    }

- Insert a newline before the opening curly brace of any code block (e.g., after `if`, `for`, `while`, `foreach`, `using`, `try`, etc.).

- Ensure that the final return statement of a method is on its own line.

- Use pattern matching and switch expressions wherever possible.

- Use `nameof` instead of string literals when referring to member names.

- Ensure that XML doc comments are created for any public APIs. When applicable, include `<example>` and `<code>` documentation in the comments. Do **not** use `global::` inside XML doc comment references (e.g., `<see cref="..."/>`) — it is unnecessary and non-idiomatic there.

- When generating multi-line string source (e.g., generated C# files or embedded code templates), follow these explicit rules:
  1. If the template does not need to embed any generator variables, prefer a plain raw string literal that starts and ends with triple quotes (`"""`). Example:

```csharp
var code = """
namespace Generated
{
    internal static partial class Helpers
    {
        // simple generated content without interpolation
    }
}
""";
```

  2. If the template needs to embed values from generator variables, use an interpolated raw string literal that starts with `$$"""` and ends with `"""`, and embed variables using `{{variableName}}` placeholders. Example:

```csharp
var code = $$"""
namespace {{Namespace}}
{
    using global::System;

    {{Accessibility}} static partial class {{ClassName}}
    {
        /// <summary>
        /// Generated extensions for {{TargetType}}
        /// </summary>
        public static global::R3.Observable<{{T}}> {{EventName}}AsObservable(this {{TargetType}} instance)
        {
            // implementation
        }
    }
}
""";
```

- Notes for raw string literals in generators:
  - The closing delimiter of a raw string literal must be placed on its own line and have the same indentation as the opening delimiter.
  - When a literal must contain a literal `{` or `}` that should not be interpreted as a placeholder, escape it by doubling (`{{` or `}}`).
  - Ensure the generator targets a C# language version that supports interpolated raw string literals; if the target environment does not support them, fall back to interpolated verbatim strings (`@$"..."`) as a compatibility measure.

- Keep generated code references fully-qualified with `global::` for public API types as required by the project-wide global-prefix rule.

## Nullable Reference Types

- Declare variables non-nullable, and check for `null` at entry points.
- Always use `is null` or `is not null` instead of `== null` or `!= null`.
- Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.

## Testing

- Always include test cases for critical paths of the application.
- Guide users through creating unit tests.
- Do not emit "Act", "Arrange" or "Assert" comments.
- Copy existing style in nearby files for test method names and capitalization.
- Demonstrate how to mock dependencies for effective testing.
