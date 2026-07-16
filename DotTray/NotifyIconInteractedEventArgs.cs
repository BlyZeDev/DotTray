namespace DotTray;

using DotTray.Primitives;

/// <summary>
/// Event arguments for a <see cref="NotifyIcon"/> interaction
/// </summary>
public sealed record NotifyIconInteractedEventArgs
{
    /// <summary>
    /// The specific type of interaction that triggered the event
    /// </summary>
    public required InteractionType Type { get; init; }
    /// <summary>
    /// The coordinates of the cursor (in screen coordinates) at the exact moment the interaction occurred
    /// </summary>
    public required Point MousePosition { get; init; }
}