namespace DotTray;

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents a collection of <see cref="MenuItem"/>
/// </summary>
public sealed class MenuItemCollection : IReadOnlyList<IMenuItem>
{
    private readonly List<IMenuItem> _items;

    internal event Action? Updated;

    /// <inheritdoc/>
    public int Count => _items.Count;

    internal MenuItemCollection() => _items = [];

    /// <inheritdoc/>
    public IMenuItem this[int index] => _items[index];

    /// <summary>
    /// Adds a <see cref="MenuItem"/> to the collection
    /// </summary>
    /// <param name="text">The display text</param>
    /// <returns><see cref="MenuItem"/></returns>
    public MenuItem Add(string text)
    {
        var item = new MenuItem(text);
        ((IMenuItem)item).Updated += Update;

        _items.Add(item);
        Update();

        return item;
    }

    /// <summary>
    /// Adds a <see cref="SeparatorItem"/> to the collection
    /// </summary>
    /// <returns><see cref="SeparatorItem"/></returns>
    public SeparatorItem AddSeparator()
    {
        var item = new SeparatorItem();
        ((IMenuItem)item).Updated += Update;

        _items.Add(item);
        Update();

        return item;
    }

    /// <summary>
    /// Removes a <see cref="MenuItem"/> at a specified index
    /// </summary>
    /// <param name="index">The index to remove the item at</param>
    public void RemoveAt(int index)
    {
        _items[index].Updated -= Update;
        _items.RemoveAt(index);
    }

    /// <summary>
    /// Clears the collection
    /// </summary>
    public void Clear()
    {
        foreach (var item in _items)
        {
            item.Updated -= Update;
        }

        _items.Clear();
    }

    /// <inheritdoc/>
    public IEnumerator<IMenuItem> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void Update() => Updated?.Invoke();
}