namespace DotTray.Popup.Default;

using DotTray.Primitives;

/// <summary>
/// Event arguments for a <see cref="MenuItemBase"/> interaction
/// </summary>
public sealed record ItemInteractedEventArgs
{
    /// <summary>
    /// The specific type of interaction that triggered the event
    /// </summary>
    public required ItemInteractionType Type { get; init; }

    /// <summary>
    /// The cursor position at the time of the interaction, relative to the item's own top-left corner
    /// </summary>
    /// <remarks>
    /// For <see cref="ItemInteractionType.MouseLeave"/>, this is the last recorded position inside the item
    /// </remarks>
    public required Point Position { get; init; }

    /// <summary>
    /// Set to <see langword="true"/> to keep the popup menu open after this interaction,
    /// overriding the default close behaviour of <see cref="ItemInteractionType.MouseLeftUp"/>
    /// and <see cref="ItemInteractionType.KeyboardActivate"/>
    /// </summary>
    public bool KeepMenuOpen { get; set; }
}