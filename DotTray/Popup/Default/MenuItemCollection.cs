namespace DotTray.Popup.Default;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Represents a collection of menu items
/// </summary>
public sealed class MenuItemCollection : IReadOnlyList<MenuItemBase>
{
    private readonly Lock _syncRoot;
    private readonly List<MenuItemBase> _items;

    /// <inheritdoc/>
    public int Count
    {
        get
        {
            lock (_syncRoot)
            {
                return _items.Count;
            }
        }
    }

    /// <summary>
    /// Returns <see langword="true"/> if the collection is empty, otherwise <see langword="false"/>
    /// </summary>
    public bool IsEmpty
    {
        get
        {
            lock (_syncRoot)
            {
                return _items.Count == 0;
            }
        }
    }

    /// <summary>
    /// Fired if this collection or an item is updated
    /// </summary>
    public event Action? Updated;

    internal MenuItemCollection()
    {
        _syncRoot = new Lock();
        _items = [];
    }

    /// <inheritdoc/>
    public MenuItemBase this[int index]
    {
        get
        {
            lock (_syncRoot)
            {
                return _items[index];
            }
        }
    }

    /// <summary>
    /// Gets the item at the specified index cast to the specified type
    /// </summary>
    /// <typeparam name="TItem">The type at the specific index</typeparam>
    /// <param name="index">The zero-based index of the element to get</param>
    /// <returns>Anything that derives from <see cref="MenuItemBase"/></returns>
    /// <exception cref="InvalidCastException"></exception>
    public TItem GetAs<TItem>(int index) where TItem : MenuItemBase => (TItem)this[index];

    /// <summary>
    /// Searches for the specified item and returns the zero-based index of the first occurrence within the entire collection
    /// </summary>
    /// <param name="item">The item to look for</param>
    /// <returns><see cref="int"/></returns>
    public int IndexOf(MenuItemBase item)
    {
        lock (_syncRoot)
        {
            return _items.IndexOf(item);
        }
    }

    /// <summary>
    /// Adds a new item to the collection
    /// </summary>
    /// <typeparam name="TItem">The type of the item</typeparam>
    public void Add<TItem>() where TItem : MenuItemBase, new()
    {
        var item = new TItem();

        item.Updated += OnUpdate;

        lock (_syncRoot)
        {
            _items.Add(item);
        }

        OnUpdate();
    }

    /// <summary>
    /// Adds a new item with the specified configuration to the collection
    /// </summary>
    /// <typeparam name="TItem">The type of the item</typeparam>
    /// <param name="configuration">The configuration of the item</param>
    public void Add<TItem>(Action<TItem> configuration) where TItem : MenuItemBase, new()
    {
        var item = new TItem();
        configuration(item);

        item.Updated += OnUpdate;

        lock (_syncRoot)
        {
            _items.Add(item);
        }

        OnUpdate();
    }

    /// <summary>
    /// Inserts an element into the collection at the specified index
    /// </summary>
    /// <typeparam name="TItem">The type of the item</typeparam>
    /// <param name="index">The zero-based index of the position to insert the item into</param>
    /// <param name="configuration">The configuration of the item</param>
    public void Insert<TItem>(int index, Action<TItem>? configuration = null) where TItem : MenuItemBase, new()
    {
        var item = new TItem();
        configuration?.Invoke(item);

        item.Updated += OnUpdate;

        lock (_syncRoot)
        {
            _items.Insert(index, item);
        }

        OnUpdate();
    }

    /// <summary>
    /// Moves the element at <paramref name="fromIndex"/> to <paramref name="toIndex"/>
    /// </summary>
    /// <param name="fromIndex">The index of the item to move</param>
    /// <param name="toIndex">The index to move the item to</param>
    public void Move(int fromIndex, int toIndex)
    {
        lock (_syncRoot)
        {
            var item = _items[fromIndex];
            _items.RemoveAt(fromIndex);
            _items.Insert(toIndex, item);
        }
        
        OnUpdate();
    }

    /// <summary>
    /// Removes the specified element from the collection
    /// </summary>
    /// <param name="item">The item to remove</param>
    /// <returns><see cref="bool"/></returns>
    public bool Remove(MenuItemBase item)
    {
        bool removed;

        lock (_syncRoot)
        {
            removed = _items.Remove(item);
        }

        if (!removed) return false;

        item.Updated -= OnUpdate;

        OnUpdate();
        return true;
    }

    /// <summary>
    /// Removes the element at the specified index from the collection
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove</param>
    public void RemoveAt(int index)
    {
        MenuItemBase item;

        lock (_syncRoot)
        {
            item = _items[index];
            _items.RemoveAt(index);
        }

        item.Updated -= OnUpdate;

        OnUpdate();
    }

    /// <summary>
    /// Removes all elements that match the condition from the collection
    /// </summary>
    /// <param name="predicate">The condition to match</param>
    /// <returns><see cref="int"/></returns>
    public int RemoveAll(Predicate<MenuItemBase> predicate)
    {
        List<MenuItemBase> toRemove;

        lock (_syncRoot)
        {
            toRemove = _items.FindAll(predicate);
            foreach (var item in toRemove) _items.Remove(item);
        }

        foreach (var item in toRemove) item.Updated -= OnUpdate;
        if (toRemove.Count > 0) OnUpdate();

        return toRemove.Count;
    }

    /// <summary>
    /// Removes all elements from the collection
    /// </summary>
    public void Clear()
    {
        List<MenuItemBase> removed;

        lock (_syncRoot)
        {
            removed = [.. _items];
            _items.Clear();
        }

        foreach (var item in removed) item.Updated -= OnUpdate;

        OnUpdate();
    }

    /// <inheritdoc/>
    public IEnumerator<MenuItemBase> GetEnumerator()
    {
        List<MenuItemBase> snapshot;

        lock (_syncRoot)
        {
            snapshot = [.. _items];
        }

        return snapshot.GetEnumerator();
    }

    internal MenuItemBase[] GetSnapshot()
    {
        lock (_syncRoot)
        {
            return [.. _items];
        }
    }

    private void OnUpdate() => Updated?.Invoke();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}