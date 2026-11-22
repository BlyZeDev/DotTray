namespace DotTray.Direct.Internal;

using DotTray.Direct.Primitives;
using System;

internal sealed class WindowLayout
{
    private readonly ItemLayout[] _items;

    public Dim Size { get; }

    public ReadOnlySpan<ItemLayout> Items => _items;

    public WindowLayout(Dim size, params ItemLayout[] items)
    {
        Size = size;
        _items = items;
    }
}