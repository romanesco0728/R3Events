namespace MarkerAttributeConflictRepro.Tests;

/// <summary>
/// Touches the generated marker attribute so the compiler has to choose between the
/// local generated definition and the imported friend-assembly definition.
/// </summary>
internal static class MarkerAttributeUsage
{
    /// <summary>
    /// Holds the marker attribute type chosen by the compiler.
    /// </summary>
    internal static global::System.Type MarkerAttributeType => typeof(global::R3Events.R3EventAttribute);
}
