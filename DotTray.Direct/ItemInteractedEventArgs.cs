namespace DotTray.Direct;

using DotTray.Direct.Items;
using DotTray.Direct.Primitives;

/// <summary>
/// Includes data about an interaction with a <see cref="MenuItemBase"/>
/// </summary>
public sealed record ItemInteractedEventArgs
{
    /// <summary>
    /// The kind of interaction
    /// </summary>
    public ItemInteractionType Type { get; init; }

    /// <summary>
    /// The position of the interaction relative to the top left of the item, in DIPs
    /// </summary>
    /// <remarks>
    /// For keyboard interactions this is the center of the item
    /// </remarks>
    public Pos Position { get; init; }

    /// <summary>
    /// <see langword="true"/> to keep the menu open after a click or keyboard activation, otherwise <see langword="false"/>
    /// </summary>
    public bool KeepMenuOpen { get; set; }
}
