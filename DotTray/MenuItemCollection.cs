namespace DotTray;

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents a <see cref="IMenuItem"/> collection
/// </summary>
public sealed class MenuItemCollection : IList<IMenuItem>
{
    private readonly IList<IMenuItem> _items;

    internal event Action? EntriesChanged;

    /// <inheritdoc/>
    public int Count => _items.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <summary>
    /// Initializes a empty <see cref="MenuItemCollection"/>
    /// </summary>
    public MenuItemCollection() : this([]) { }

    /// <summary>
    /// Initializes a <see cref="MenuItemCollection"/> from an existing <see cref="IEnumerable{T}"/>
    /// </summary>
    /// <param name="menuItems"><see cref="IEnumerable{T}"/> to create the <see cref="MenuItemCollection"/> from</param>
    public MenuItemCollection(IEnumerable<IMenuItem> menuItems)
    {
        _items = [];

        foreach (var item in menuItems)
        {
            _items.Add(item);
        }

        EntriesChanged?.Invoke();
    }

    /// <inheritdoc/>
    public IMenuItem this[int index]
    {
        get => _items[index];
        set
        {
            _items[index] = value;
            EntriesChanged?.Invoke();
        }
    }

    /// <inheritdoc/>
    public void Add(IMenuItem item)
    {
        _items.Add(item);
        EntriesChanged?.Invoke();
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _items.Clear();
        EntriesChanged?.Invoke();
    }

    /// <inheritdoc/>
    public bool Contains(IMenuItem item) => _items.Contains(item);

    /// <inheritdoc/>
    public void CopyTo(IMenuItem[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public int IndexOf(IMenuItem item) => _items.IndexOf(item);

    /// <inheritdoc/>
    public void Insert(int index, IMenuItem item)
    {
        _items.Insert(index, item);
        EntriesChanged?.Invoke();
    }

    /// <inheritdoc/>
    public bool Remove(IMenuItem item)
    {
        var removed = _items.Remove(item);
        EntriesChanged?.Invoke();
        return removed;
    }

    /// <inheritdoc/>
    public void RemoveAt(int index)
    {
        _items.RemoveAt(index);
        EntriesChanged?.Invoke();
    }

    /// <inheritdoc/>
    public IEnumerator<IMenuItem> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}