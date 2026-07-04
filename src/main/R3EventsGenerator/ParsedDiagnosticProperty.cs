using Microsoft.CodeAnalysis;
using R3EventsGenerator.Utilities;

namespace R3EventsGenerator;

/// <summary>
/// Represents declaration metadata and source locations used when reporting diagnostics for an attributed class.
/// </summary>
internal sealed record ParsedDiagnosticProperty
{
    /// <summary>
    /// Gets the attributed class name model containing both code-qualified and user-facing representations.
    /// </summary>
    public required TypeNameView ClassNameView { get; init; }
    /// <summary>
    /// Gets the location of the R3EventAttribute application site (the attribute node in source).
    /// Used to position diagnostics and code-fix actions at the attribute rather than the class declaration.
    /// </summary>
    public required IgnoreEquality<Location> AttributeLocation { get; init; }
    /// <summary>
    /// Gets the deterministic comparison key for the attribute application location.
    /// </summary>
    public required LocationKey AttributeLocationKey { get; init; }
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
    /// Gets the location of the class identifier for diagnostics related to class declaration requirements.
    /// </summary>
    public required IgnoreEquality<Location> PartialLocation { get; init; }
    /// <summary>
    /// Gets the deterministic comparison key for the class identifier location.
    /// </summary>
    public required LocationKey PartialLocationKey { get; init; }
}