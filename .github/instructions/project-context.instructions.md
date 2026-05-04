---
description: 'Project context for R3Events — a Roslyn source generator that bridges .NET events with R3 reactive programming.'
applyTo: '**'
---

# R3Events Project Context

## What is R3Events?

R3Events is a **C# Roslyn source generator** library that automatically generates `AsObservable()` extension methods for every public event on a target type, enabling seamless integration with the [R3](https://github.com/Cysharp/R3) reactive programming library.

**Usage:** Annotate a `static partial class` with `[R3Event(typeof(T))]` and the generator produces extension methods for all public events on `T`.

```csharp
[R3Event(typeof(MyControl))]
static partial class MyControlExtensions { }

// Generated: MyControlExtensions.ClickAsObservable(this MyControl), etc.
```

## Repository Structure

```
src/
  main/
    R3EventsGenerator/            # Roslyn incremental source generator implementation
    R3EventsGenerator.Attributes/ # [R3Event] attribute definitions (consumed by user code and the generator)
  tests/                          # Unit and integration tests for the generator
.github/
  instructions/                   # Copilot coding instructions (this folder)
  workflows/                      # CI/CD pipelines
```

## Key Design Decisions

- Uses **Roslyn Incremental Generator** (`IIncrementalGenerator`) for performance.
- All types in generated output are referenced with `global::` fully-qualified names to prevent collisions in consumer projects.
- Generated partial class declarations omit access modifiers — accessibility is set by the hand-authored declaration.
- The NuGet package ships both the attribute assembly and the generator assembly.
