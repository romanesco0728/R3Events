using Microsoft.CodeAnalysis;

namespace R3EventsGenerator;

/// <summary>
/// Represents a deterministic key for Roslyn locations used in incremental equality comparisons.
/// </summary>
internal readonly record struct LocationKey(string FilePath, int Start, int Length, bool IsInSource)
{
    /// <summary>
    /// Creates a stable key from the specified location.
    /// </summary>
    /// <param name="location">The location to convert into a key.</param>
    /// <returns>A key containing file path and span information.</returns>
    public static LocationKey From(Location location)
    {
        if (!location.IsInSource || location.SourceTree is null)
        {
            return new(string.Empty, 0, 0, false);
        }

        var span = location.SourceSpan;
        return new(location.SourceTree.FilePath, span.Start, span.Length, true);
    }
}