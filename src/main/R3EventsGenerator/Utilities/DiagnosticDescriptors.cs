using Microsoft.CodeAnalysis;

namespace R3EventsGenerator.Utilities;

internal static class DiagnosticDescriptors
{
    const string Category = "R3Events";

    public static readonly DiagnosticDescriptor MustBePartial = new(
        id: "R3E001",
        title: "Target type must be partial",
        messageFormat: "Type '{0}' must be declared as partial to use R3EventAttribute.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor MustNotBeNested = new(
        id: "R3E002",
        title: "Target type must not be nested",
        messageFormat: "Type '{0}' must be a top-level class to use R3EventAttribute.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor MustBeStatic = new(
        id: "R3E003",
        title: "Target type must be static",
        messageFormat: "Type '{0}' must be declared as static to use R3EventAttribute.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor MustNotBeGeneric = new(
        id: "R3E004",
        title: "Target type must not be generic",
        messageFormat: "Type '{0}' must not be a generic type to use R3EventAttribute.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor PreferGenericAttribute = new(
        id: "R3W001",
        title: "Prefer generic R3EventAttribute<T>",
        messageFormat: "Type '{0}' uses R3EventAttribute(typeof(T)). Consider using R3EventAttribute<T> instead, which is available in C# 11 and later.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
}
