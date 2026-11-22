namespace DotTray.Direct.Internal;

using DotTray.Direct.Items;
using DotTray.Direct.Primitives;

internal readonly record struct ItemLayout
{
    public required MenuItemBase Item { get; init; }

    public required Rect Bounds { get; init; }
}