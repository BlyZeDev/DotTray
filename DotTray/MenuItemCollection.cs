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

    internal event Action? EntriesChanged;

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
        item.Changed += Update;

        _items.Add(item);
        Update();

        return item;
    }

    /// <summary>
    /// Adds a <see cref="SeparatorItem"/> to the collection
    /// </summary>
    public void AddSeparator() => _items.Add(SeparatorItem.Instance);

    /// <summary>
    /// Removes a <see cref="MenuItem"/> at a specified index
    /// </summary>
    /// <param name="index">The index to remove the item at</param>
    public void RemoveAt(int index)
    {
        var item = _items[index];
        if (item is MenuItem menuItem) menuItem.Changed -= Update;

        _items.RemoveAt(index);
        Update();
    }

    /// <summary>
    /// Clears the collection
    /// </summary>
    public void Clear()
    {
        foreach (var item in _items)
        {
            if (item is MenuItem menuItem) menuItem.Changed -= Update;
        }

        _items.Clear();
        Update();
    }

    /// <inheritdoc/>
    public IEnumerator<IMenuItem> GetEnumerator() => _items.GetEnumerator();

    private void Update() => EntriesChanged?.Invoke();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}