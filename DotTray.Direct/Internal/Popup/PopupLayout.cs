namespace DotTray.Direct.Internal.Popup;

using DotTray.Direct.Context;
using DotTray.Direct.Internal.Rendering;
using DotTray.Direct.Items;
using DotTray.Direct.Primitives;

internal sealed class PopupLayout
{
    public static readonly PopupLayout Empty = new PopupLayout(Dim.Empty, []);

    private readonly ItemLayout[] _items;

    public Dim Size { get; }

    public ReadOnlySpan<ItemLayout> Items => _items;

    private PopupLayout(Dim size, ItemLayout[] items)
    {
        Size = size;
        _items = items;
    }

    public static PopupLayout Measure(ReadOnlySpan<MenuItemBase> items, MeasuringContext measuring)
    {
        var maxWidth = 0;
        var totalHeight = 0;
        Span<int> heights = items.Length > 16 ? new int[items.Length] : stackalloc int[items.Length];

        for (var i = 0; i < items.Length; i++)
        {
            var size = items[i].Measure(measuring);

            heights[i] = size.Height;
            maxWidth = Math.Max(maxWidth, size.Width);
            totalHeight += size.Height;
        }

        var layout = new ItemLayout[items.Length];
        var itemTop = 0;

        for (var i = 0; i < items.Length; i++)
        {
            layout[i] = new ItemLayout
            {
                Item = items[i],
                Bounds = new Rect(0, itemTop, maxWidth, heights[i])
            };

            itemTop += heights[i];
        }

        return new PopupLayout(new Dim(maxWidth, totalHeight), layout);
    }

    public int IndexOf(MenuItemBase? item)
    {
        if (item is null) return -1;

        for (var i = 0; i < _items.Length; i++)
        {
            if (ReferenceEquals(_items[i].Item, item)) return i;
        }

        return -1;
    }

    public Rect BoundsOf(MenuItemBase item)
    {
        var index = IndexOf(item);
        return index >= 0 ? _items[index].Bounds : default;
    }

    public int HitTest(Pos pixel, float scale)
    {
        for (var i = 0; i < _items.Length; i++)
        {
            if (_items[i].Item.IgnoreInteraction) continue;

            var bounds = _items[i].Bounds;
            if (pixel.X >= PixelGrid.ToPixel(bounds.Left, scale) && pixel.X < PixelGrid.ToPixel(bounds.Right, scale)
                && pixel.Y >= PixelGrid.ToPixel(bounds.Top, scale) && pixel.Y < PixelGrid.ToPixel(bounds.Bottom, scale)) return i;
        }

        return -1;
    }
}