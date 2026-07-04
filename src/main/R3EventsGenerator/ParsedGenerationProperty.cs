using R3EventsGenerator.Utilities;

namespace R3EventsGenerator;

/// <summary>
/// Represents metadata for a class and its associated generated Observable extension methods as parsed from an
/// R3EventAttribute.
/// </summary>
internal sealed record ParsedGenerationProperty
{
    /// <summary>
    /// Gets the fully qualified namespace of the attributed class, or an empty string if the class is in the global namespace.
    /// </summary>
    public required string ClassNamespace { get; init; }
    /// <summary>
    /// Gets the name of the attributed class.
    /// </summary>
    public required string ClassName { get; init; }
    /// <summary>
    /// Gets a collection of metadata describing the generated Observable extension methods for the attributed class.
    /// </summary>
    public required EquatableArray<GeneratedMethodInfo> GeneratedMethods { get; init; }
    /// <summary>
    /// Gets the attributed class name model containing both code-qualified and user-facing representations.
    /// </summary>
    public required TypeNameView ClassNameView { get; init; }
    /// <summary>
    /// Gets the target type name model containing both code-qualified and user-facing representations.
    /// </summary>
    public required TypeNameView TargetTypeName { get; init; }
    /// <summary>
    /// Gets a value indicating whether the attributed class is nested within another type.
    /// </summary>
    public required bool IsNested { get; init; }
    /// <summary>
    /// Gets a value indicating whether the attributed class is declared as static.
    /// </summary>
    public required bool IsStatic { get; init; }
    /// <summary>
    /// Gets a value indicating whether the attributed class is a generic type.
    /// </summary>
    public required bool IsGeneric { get; init; }
    /// <summary>
    /// Gets a value indicating whether the attributed class is declared as partial.
    /// </summary>
    public required bool IsPartial { get; init; }
    /// <summary>
    /// Gets the base hint name used for generated source file names.
    /// </summary>
    public string HintBaseName => (string.IsNullOrEmpty(ClassNamespace) ? ClassName : $"{ClassNamespace}.{ClassName}")
        .Replace("global::", "")
        .Replace("<", "_")
        .Replace(">", "_");
}