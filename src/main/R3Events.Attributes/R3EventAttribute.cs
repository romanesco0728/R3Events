namespace R3Events;

/// <summary>
/// Marks a static partial class for automatic generation of R3 Observable extension methods
/// for all public events declared on the specified target type.
/// </summary>
/// <param name="type">The target <see cref="Type"/> whose events will be exposed as Observable extension methods.</param>
/// <remarks>
/// Prefer the generic variant <see cref="R3EventAttribute{T}"/> when using C# 11 or later.
/// </remarks>
/// <remarks>
/// The <see cref="System.Diagnostics.ConditionalAttribute"/> with the symbol
/// <c>R3EVENTS_ATTRIBUTE_USAGES</c> (which is never defined) causes the compiler to strip
/// all usages of this attribute from the emitted IL. This means the attribute assembly is only
/// required at compile time and does not need to be present at runtime.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
[System.Diagnostics.Conditional("R3EVENTS_ATTRIBUTE_USAGES")]
public sealed class R3EventAttribute(Type type) : Attribute
{
    /// <summary>
    /// Gets the target type whose events will be exposed as Observable extension methods.
    /// </summary>
    public Type Type { get; } = type ?? throw new ArgumentNullException(nameof(type));
}

/// <summary>
/// Marks a static partial class for automatic generation of R3 Observable extension methods
/// for all public events declared on the specified target type.
/// </summary>
/// <typeparam name="T">The target type whose events will be exposed as Observable extension methods.</typeparam>
/// <remarks>
/// Requires C# 11 or later for generic attribute syntax.
/// </remarks>
/// <remarks>
/// The <see cref="System.Diagnostics.ConditionalAttribute"/> with the symbol
/// <c>R3EVENTS_ATTRIBUTE_USAGES</c> (which is never defined) causes the compiler to strip
/// all usages of this attribute from the emitted IL. This means the attribute assembly is only
/// required at compile time and does not need to be present at runtime.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
[System.Diagnostics.Conditional("R3EVENTS_ATTRIBUTE_USAGES")]
public sealed class R3EventAttribute<T> : Attribute
{
}
