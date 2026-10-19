namespace DotTray.Direct;

using DotTray.Direct.Primitives;
using System;

/// <summary>
/// The layout of a popup window: its content size and where every item is
/// </summary>
/// <remarks>
/// Layouts are immutable results. Items that are not part of the layout are neither drawn nor interactive
/// </remarks>
public sealed class WindowLayout
{
    private readonly ItemLayout[] _items;

    /// <summary>
    /// The size of the window content
    /// </summary>
    public Dim Size { get; }

    /// <summary>
    /// The arranged items, in the order they should be drawn and navigated
    /// </summary>
    public ReadOnlySpan<ItemLayout> Items => _items;

    /// <summary>
    /// Creates a new <see cref="WindowLayout"/>
    /// </summary>
    /// <param name="size">The size of the window content in DIPs</param>
    /// <param name="items">The arranged items</param>
    public WindowLayout(Dim size, params ItemLayout[] items)
    {
        Size = size;
        _items = items;
    }
}