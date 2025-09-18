namespace DotTray;

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents a <see cref="MenuItem"/> collection
/// </summary>
public sealed class MenuItemCollection : IReadOnlyList<MenuItem>
{
    private readonly List<MenuItem> _items;

    internal event Action? EntriesChanged;

    /// <inheritdoc/>
    public int Count => _items.Count;

    internal MenuItemCollection() => _items = [];

    /// <inheritdoc/>
    public MenuItem this[int index] => _items[index];

    /// <summary>
    /// Adds a new menu item and returns it
    /// </summary>
    /// <typeparam name="T">The type of menu item to add</typeparam>
    /// <param name="text">The text of the added menu item</param>
    /// <returns><see cref="MenuItem"/></returns>
    public MenuItem Add<T>(string text) where T : MenuItem
    {


        _items.Add(newItem);
        EntriesChanged?.Invoke();

        return newItem;
    }

    /// <summary>
    /// Clears the collection
    /// </summary>
    public void Clear()
    {
        _items.Clear();
        EntriesChanged?.Invoke();
    }

    /// <inheritdoc/>
    public bool Contains(MenuItem item) => _items.Contains(item);

    /// <inheritdoc/>
    public void CopyTo(MenuItem[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public int IndexOf(MenuItem item) => _items.IndexOf(item);

    /// <inheritdoc/>
    public void Insert(int index, MenuItem item)
    {
        _items.Insert(index, item);
        EntriesChanged?.Invoke();
    }

    /// <inheritdoc/>
    public void RemoveAt(int index)
    {
        _items.RemoveAt(index);
        EntriesChanged?.Invoke();
    }

    /// <inheritdoc/>
    public IEnumerator<MenuItem> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}