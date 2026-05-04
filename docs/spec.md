# Specification: R3Events.R3EventAttribute and Auto-Generation Behavior

## 1. Purpose

This specification defines the behavior of the `R3Events.R3EventAttribute` attribute type emitted by the Incremental Source Generator, and the extension-method auto-generation triggered by that attribute. The generator detects public events on the target type and generates extension methods returning `global::R3.Observable<T>`.

## 2. Attribute: R3Events.R3EventAttribute

- **Kind**: An `internal sealed` class inheriting `global::System.Attribute`.
- **Target**: Applicable to classes only. Emitted code must declare:
  `global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)`
- **Namespace**: `R3Events` (matching the namespace in generated code).

### 2.1. Non-generic attribute (C# 8 and later)

- Constructor: `public R3EventAttribute(global::System.Type type)` — specifies the target type.
- Property: `public global::System.Type Type { get; }`
- Usage: `[R3EventAttribute(typeof(MyClass))] internal static partial class MyClassExtensions;`

### 2.2. Generic attribute (C# 11 and later only)

- The generator emits the generic variant `R3EventAttribute<T>` only when the project's language version is C# 11 or later.
- Constructor: `public R3EventAttribute()` — the target type is specified via the type parameter `T`.
- Property: `public global::System.Type Type { get; }` — derived from `typeof(T)`.
- Usage: `[R3EventAttribute<MyClass>] internal static partial class MyClassExtensions;`
- **Compatibility**: Both forms can coexist in the same project. The generator recognizes both and produces identical extension methods.

### Language-version detection

- At compile time the generator inspects the language version across all source files and emits the generic attribute when the maximum version is C# 11 or later.
- C# 8 or later is required on the consumer side (generated code contains nullable reference type syntax).
- On C# 8–10 projects the generic attribute is not emitted; only the non-generic form is available.

> Rule: All public API type references in emitted code must use the `global::` fully-qualified prefix (see `.github/instructions/generator.instructions.md`).

## 3. Generator Trigger

Annotate any `static partial class` in the consuming project with `R3Events.R3EventAttribute` or `R3Events.R3EventAttribute<T>`:

**Non-generic attribute:**

```csharp
[R3Events.R3EventAttribute(typeof(C1))]
internal static partial class C1Extensions;
```

**Generic attribute (C# 11+ only):**

```csharp
[R3Events.R3EventAttribute<C1>]
internal static partial class C1Extensions;
```

Either form causes the generator to inspect all public events on `C1` and emit the corresponding extension methods.

## 4. Shape of Generated Extension Methods

- Naming: `{EventName}AsObservable` for each public event.
- Signature:
  ```
  public static global::R3.Observable<T> {EventName}AsObservable(
      this global::NamespaceOfTarget.TargetType instance,
      global::System.Threading.CancellationToken cancellationToken = default)
  ```
- `T` is resolved as follows:
  - `EventHandler` or `EventHandler<global::System.EventArgs>` → `global::R3.Unit`
  - `CancelEventHandler` or other `EventHandler<TEventArgs>` → `TEventArgs` (nullability preserved)
  - Non-`EventHandler` delegates → the last parameter type (EventArgs-like); falls back to `global::System.Object` when inference is not possible.
- Implementation uses R3 utilities:
  - `Observable.FromEventHandler` or `Observable.FromEvent<TDelegate, TPayload>` as appropriate.
  - Transformations such as `AsUnitObservable()` or `Select(ep => ep.Args)` are applied as needed.

## 5. Safety and Nullable Reference Types

- Generated code respects nullable reference types.
- Every generated file must include a file-scoped `#nullable enable` directive. Because generated code uses nullable annotations such as `global::System.Object?` (e.g., the event sender), this directive must be present regardless of the consumer project's nullable setting — omitting it produces CS8669.
- Public API references in generated code use the `global::` prefix.
- Generated code uses block-style namespace declarations (`namespace XXX { ... }`) and includes XML doc comments.
- **Type-name policy**: Type references in generated code bodies (signatures and implementations) retain `global::` for disambiguation. User-facing strings (diagnostic messages and display names inside XML doc comments) omit the `global::` prefix.

## 6. Dependencies

- Generated code depends on types in the `global::R3` namespace (primarily `Observable<T>`, `Unit`, and `Observable` utilities).
- `using` directives may be emitted inside the namespace block when needed, but public API references always use `global::`.

## 7. Testing

- Unit tests must verify that given an input source the generator produces the expected output.
- Tests must cover diverse event scenarios: nullable events, generic `EventHandler<T>`, and custom delegates.

## 8. Generated Class and File Naming

- The generator places extension methods in the same partial class as the one annotated with `R3Events.R3EventAttribute`, using the same namespace and class name so they merge at compile time.  
  Example: if the annotated class is `internal static partial class C1Extensions`, the generator emits a matching `namespace` and `class C1Extensions` partial declaration.

- The generated source file name is formed by joining the namespace and class name with dots and appending `.g.cs`.  
  Example: namespace `MyApp.Sample`, class `C1Extensions` → file `MyApp.Sample.C1Extensions.g.cs`.

- This ensures generated code is merged into the same type as the annotated class, preserving visibility and consistency for the consumer.

## 9. Diagnostics

The generator emits two categories of diagnostics: errors and informational messages.

### 9.1. Errors

| ID     | Condition |
| ------ | --------- |
| R3E001 | Target class is not `partial` |
| R3E002 | Target class is nested |
| R3E003 | Target class is not `static` |
| R3E004 | Target class is a generic type |

When any of these errors is reported, no code is generated.

### 9.2. Informational

| ID     | Title | Condition |
| ------ | ----- | --------- |
| R3I001 | Prefer generic R3EventAttribute\<T\> | Non-generic `R3EventAttribute(typeof(T))` is used on a project with C# 11 or later |

- **R3I001 location**: Reported at the `AttributeSyntax` node (`[R3EventAttribute(typeof(T))]`), not at the class declaration.
- R3I001 is not reported on C# 8–10 projects (non-generic is the only option there).
- R3I001 is suppressed when any of R3E001–R3E004 is also reported (errors take priority).

### 9.3. Code Fix (R3I001)

A `CodeFixProvider` quick-fix is provided for R3I001:

- Transformation: `[R3Event(typeof(T))]` → `[R3Event<T>]`
- Qualification is preserved: `R3Events.R3Event(typeof(T))` → `R3Events.R3Event<T>`
- `AliasQualifiedNameSyntax` (e.g., `global::R3Events.R3Event`) is also handled.
- Bulk fix: `WellKnownFixAllProviders.BatchFixer` supports applying the fix across the entire project at once.
