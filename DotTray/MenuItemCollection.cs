namespace DotTray;

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents a collection of <see cref="MenuItem"/>
/// </summary>
public sealed class MenuItemCollection : IReadOnlyList<MenuItemBase>
{
    private readonly List<MenuItemBase> _items;
    private readonly Action<MenuItem> _defaultMenuItemConfig;
    private readonly Action<SeparatorItem> _defaultSeparatorItemConfig;

    internal event Action? Updated;

    /// <inheritdoc/>
    public int Count => _items.Count;

    /// <summary>
    /// Returns <see langword="true"/> if the collection is empty, otherwise <see langword="false"/>
    /// </summary>
    public bool IsEmpty => _items.Count == 0;

    internal MenuItemCollection(Action<MenuItem> defaultMenuItemConfig, Action<SeparatorItem> defaultSeparatorItemConfig)
    {
        _items = [];

        _defaultMenuItemConfig = defaultMenuItemConfig;
        _defaultSeparatorItemConfig = defaultSeparatorItemConfig;
    }

    /// <inheritdoc/>
    public MenuItemBase this[int index] => _items[index];

    /// <summary>
    /// Gets the item at the specified index cast to the specified type
    /// </summary>
    /// <typeparam name="TItem">The type at the specific index</typeparam>
    /// <param name="index">The zero-based index of the element to get</param>
    /// <returns>Anything that derives from <see cref="MenuItemBase"/></returns>
    public TItem GetAt<TItem>(int index) where TItem : MenuItemBase => (TItem)_items[index];

    /// <summary>
    /// Adds a <see cref="MenuItem"/> to the collection
    /// </summary>
    /// <param name="text">The display text</param>
    /// <returns><see cref="MenuItem"/></returns>
    public MenuItem AddItem(string text)
    {
        var item = new MenuItem(text, _defaultMenuItemConfig, _defaultSeparatorItemConfig);
        _defaultMenuItemConfig(item);

        item.Updated += Update;

        _items.Add(item);
        Update();

        return item;
    }

    /// <summary>
    /// Adds a <see cref="MenuItem"/> to the collection
    /// </summary>
    /// <param name="menuItemConfig">The configuration to use</param>
    /// <returns><see cref="MenuItem"/></returns>
    public MenuItem AddItem(Action<MenuItem> menuItemConfig)
    {
        var item = new MenuItem("", _defaultMenuItemConfig, _defaultSeparatorItemConfig);
        _defaultMenuItemConfig(item);
        menuItemConfig(item);

        item.Updated += Update;

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
        _defaultSeparatorItemConfig(item);

        item.Updated += Update;

        _items.Add(item);
        Update();

        return item;
    }

    /// <summary>
    /// Adds a <see cref="SeparatorItem"/> to the collection
    /// </summary>
    /// <param name="separatorItemConfig">The configuration to use</param>
    /// <returns></returns>
    public SeparatorItem AddSeparator(Action<SeparatorItem> separatorItemConfig)
    {
        var item = new SeparatorItem();
        _defaultSeparatorItemConfig(item);
        separatorItemConfig(item);

        item.Updated += Update;

        _items.Add(item);
        Update();

        return item;
    }

    /// <summary>
    /// Removes a <see cref="MenuItemBase"/> at a specified index
    /// </summary>
    /// <param name="index">The zero-based index to remove the item at</param>
    public void RemoveAt(int index)
    {
        _items[index].Updated -= Update;
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
            item.Updated -= Update;
        }

        _items.Clear();
    }

    /// <inheritdoc/>
    public IEnumerator<MenuItemBase> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void Update() => Updated?.Invoke();
}