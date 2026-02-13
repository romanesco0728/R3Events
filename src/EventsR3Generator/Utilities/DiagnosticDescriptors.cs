using Microsoft.CodeAnalysis;

namespace EventsR3Generator.Utilities;

internal static class DiagnosticDescriptors
{
    const string Category = "Events.R3";

    public static readonly DiagnosticDescriptor MustBePartial = new(
        id: "ER001",
        title: "Target type must be partial",
        messageFormat: "Type '{0}' must be declared as partial to use R3EventAttribute.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor MustNotBeNested = new(
        id: "ER002",
        title: "Target type must not be nested",
        messageFormat: "Type '{0}' must be a top-level class to use R3EventAttribute.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}
