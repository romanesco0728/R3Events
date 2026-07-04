namespace R3EventsGenerator;

/// <summary>
/// Represents obsolete metadata copied from the source event for method emission.
/// </summary>
internal sealed record GeneratedObsoleteInfo
{
    /// <summary>
    /// Gets the obsolete message supplied by the source event, if any.
    /// </summary>
    public required string? Message { get; init; }
    /// <summary>
    /// Gets a value indicating whether the obsolete message argument was explicitly supplied.
    /// </summary>
    public required bool HasMessageArgument { get; init; }
    /// <summary>
    /// Gets a value indicating whether the obsolete error argument was explicitly supplied.
    /// </summary>
    public required bool HasErrorArgument { get; init; }
    /// <summary>
    /// Gets a value indicating whether the obsolete attribute is an error.
    /// </summary>
    public required bool IsError { get; init; }
}