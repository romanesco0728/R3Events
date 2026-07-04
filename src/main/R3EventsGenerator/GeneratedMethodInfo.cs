namespace R3EventsGenerator;

/// <summary>
/// Represents metadata describing a generated method for an observable event, including event name, element type,
/// and delegate information.
/// </summary>
internal sealed record GeneratedMethodInfo
{
    /// <summary>
    /// Gets the name of the event associated with this instance.
    /// </summary>
    public required string EventName { get; init; }
    /// <summary>
    /// Gets the name of the element type that is being observed.
    /// </summary>
    public required TypeNameView ObservableElementType { get; init; }
    /// <summary>
    /// Gets a value indicating whether to use R3.Unit as the element type for the observable.
    /// </summary>
    public required bool UseAsUnit { get; init; }
    /// <summary>
    /// Gets the fully qualified name of the delegate type used for the event handler.
    /// </summary>
    public required TypeNameView DelegateType { get; init; }
    /// <summary>
    /// Gets the obsolete metadata copied from the source event when present.
    /// </summary>
    public required GeneratedObsoleteInfo? ObsoleteInfo { get; init; }
}