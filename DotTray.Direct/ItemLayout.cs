namespace DotTray.Direct;

using DotTray.Direct.Items;
using DotTray.Direct.Primitives;

/// <summary>
/// The result of arranging one <see cref="MenuItemBase"/> inside a popup window
/// </summary>
public readonly record struct ItemLayout
{
    /// <summary>
    /// The arranged item
    /// </summary>
    public required MenuItemBase Item { get; init; }

    /// <summary>
    /// The full bounds of the item, used for hit testing and as the clip while drawing
    /// </summary>
    public required Rect Bounds { get; init; }
}