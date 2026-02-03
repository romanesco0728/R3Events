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

- Ensure that XML doc comments are created for any public APIs. When applicable, include `<example>` and `<code>` documentation in the comments.

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

## Project Setup and Structure

- Guide users through creating a new .NET project with the appropriate templates.
- Explain the purpose of each generated file and folder to build understanding of the project structure.
- Demonstrate how to organize code using feature folders or domain-driven design principles.
- Show proper separation of concerns with models, services, and data access layers.
- Explain the Program.cs and configuration system in ASP.NET Core 10 including environment-specific settings.

## Nullable Reference Types

- Declare variables non-nullable, and check for `null` at entry points.
- Always use `is null` or `is not null` instead of `== null` or `!= null`.
- Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.

## Data Access Patterns

- Guide the implementation of a data access layer using Entity Framework Core.
- Explain different options (SQL Server, SQLite, In-Memory) for development and production.
- Demonstrate repository pattern implementation and when it's beneficial.
- Show how to implement database migrations and data seeding.
- Explain efficient query patterns to avoid common performance issues.

## Authentication and Authorization

- Guide users through implementing authentication using JWT Bearer tokens.
- Explain OAuth 2.0 and OpenID Connect concepts as they relate to ASP.NET Core.
- Show how to implement role-based and policy-based authorization.
- Demonstrate integration with Microsoft Entra ID (formerly Azure AD).
- Explain how to secure both controller-based and Minimal APIs consistently.

## Validation and Error Handling

- Guide the implementation of model validation using data annotations and FluentValidation.
- Explain the validation pipeline and how to customize validation responses.
- Demonstrate a global exception handling strategy using middleware.
- Show how to create consistent error responses across the API.
- Explain problem details (RFC 9457) implementation for standardized error responses.

## API Versioning and Documentation

- Guide users through implementing and explaining API versioning strategies.
- Demonstrate Swagger/OpenAPI implementation with proper documentation.
- Show how to document endpoints, parameters, responses, and authentication.
- Explain versioning in both controller-based and Minimal APIs.
- Guide users on creating meaningful API documentation that helps consumers.

## Logging and Monitoring

- Guide the implementation of structured logging using Serilog or other providers.
- Explain the logging levels and when to use each.
- Demonstrate integration with Application Insights for telemetry collection.
- Show how to implement custom telemetry and correlation IDs for request tracking.
- Explain how to monitor API performance, errors, and usage patterns.

## Testing

- Always include test cases for critical paths of the application.
- Guide users through creating unit tests.
- Do not emit "Act", "Arrange" or "Assert" comments.
- Copy existing style in nearby files for test method names and capitalization.
- Explain integration testing approaches for API endpoints.
- Demonstrate how to mock dependencies for effective testing.
- Show how to test authentication and authorization logic.
- Explain test-driven development principles as applied to API development.

## Performance Optimization

- Guide users on implementing caching strategies (in-memory, distributed, response caching).
- Explain asynchronous programming patterns and why they matter for API performance.
- Demonstrate pagination, filtering, and sorting for large data sets.
- Show how to implement compression and other performance optimizations.
- Explain how to measure and benchmark API performance.

## Deployment and DevOps

- Guide users through containerizing their API using .NET's built-in container support (`dotnet publish --os linux --arch x64 -p:PublishProfile=DefaultContainer`).
- Explain the differences between manual Dockerfile creation and .NET's container publishing features.
- Explain CI/CD pipelines for NET applications.
- Demonstrate deployment to Azure App Service, Azure Container Apps, or other hosting options.
- Show how to implement health checks and readiness probes.
- Explain environment-specific configurations for different deployment stages.
