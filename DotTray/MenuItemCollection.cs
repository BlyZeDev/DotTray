namespace DotTray;

using DotTray.Popup;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents a collection of menu items
/// </summary>
public sealed class MenuItemCollection : IReadOnlyList<MenuItemBase>
{
    private readonly List<MenuItemBase> _items;

    /// <inheritdoc/>
    public int Count => _items.Count;

    /// <summary>
    /// Returns <see langword="true"/> if the collection is empty, otherwise <see langword="false"/>
    /// </summary>
    public bool IsEmpty => _items.Count == 0;

    internal MenuItemCollection()
    {
        _items = [];
    }

    /// <inheritdoc/>
    public MenuItemBase this[int index] => _items[index];

    /// <summary>
    /// Gets the item at the specified index cast to the specified type
    /// </summary>
    /// <typeparam name="TItem">The type at the specific index</typeparam>
    /// <param name="index">The zero-based index of the element to get</param>
    /// <returns>Anything that derives from <see cref="MenuItemBase"/></returns>
    /// <exception cref="InvalidCastException"></exception>
    public TItem GetAt<TItem>(int index) where TItem : MenuItemBase => (TItem)this[index];

    /// <summary>
    /// Adds a new item with the specified configuration to the collection
    /// </summary>
    /// <typeparam name="TItem">The type of the item</typeparam>
    /// <param name="configuration">The configuration of the item</param>
    public void Add<TItem>(Action<TItem> configuration) where TItem : MenuItemBase, new()
    {
        var item = new TItem();
        configuration(item);
        _items.Add(item);
    }

    /// <summary>
    /// Adds multiple new items with the specified configurations to the collection
    /// </summary>
    /// <typeparam name="TItem">The type of the item</typeparam>
    /// <param name="configurations">The configurations of the items</param>
    public void AddRange<TItem>(params ReadOnlySpan<Action<TItem>> configurations) where TItem : MenuItemBase, new()
    {
        foreach (var configuration in configurations)
        {
            Add(configuration);
        }
    }

    /// <summary>
    /// Removes the element at the specified index
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove</param>
    public void RemoveAt(int index) => _items.RemoveAt(index);

    /// <inheritdoc/>
    public IEnumerator<MenuItemBase> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}