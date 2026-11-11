namespace DotTray.Direct.Items;

/// <summary>
/// Represents a group of <see cref="RadioItem"/> instances of which at most one can be selected at a time
/// </summary>
/// <remarks>
/// Items join a group by setting <see cref="RadioItem.Group"/> or by calling <see cref="Add(RadioItem)"/>.<br/>
/// A group can span multiple <see cref="MenuItemCollection"/> instances, eg. items in a submenu
/// </remarks>
public sealed class RadioItemGroup
{
    private readonly Lock _syncRoot;
    private readonly List<RadioItem> _items;

    private RadioItem? selected;

    /// <summary>
    /// The currently selected item, or <see langword="null"/> if none is selected
    /// </summary>
    public RadioItem? SelectedItem
    {
        get
        {
            lock (_syncRoot)
            {
                return selected;
            }
        }
    }

    /// <summary>
    /// A snapshot of the items that are currently part of this group
    /// </summary>
    public IReadOnlyList<RadioItem> Items
    {
        get
        {
            lock (_syncRoot)
            {
                return _items.ToArray();
            }
        }
    }

    /// <summary>
    /// Fired when the selection changed, with the newly selected item or <see langword="null"/> if the selection was cleared
    /// </summary>
    /// <remarks>
    /// <b>Note:</b> This action is fired on the <see cref="NotifyIcon"/>'s background STA thread
    /// </remarks>
    public event Action<RadioItem?>? SelectionChanged;

    /// <summary>
    /// Default configuration for <see cref="RadioItemGroup"/>
    /// </summary>
    public RadioItemGroup()
    {
        _syncRoot = new Lock();
        _items = [];
    }

    /// <summary>
    /// Adds <paramref name="item"/> to this group, which removes it from its previous group
    /// </summary>
    /// <remarks>
    /// If <paramref name="item"/> is already checked, it becomes the selected item
    /// </remarks>
    /// <param name="item">The item to add</param>
    public void Add(RadioItem item) => item.Group = this;

    /// <summary>
    /// Removes <paramref name="item"/> from this group, which unchecks it
    /// </summary>
    /// <param name="item">The item to remove</param>
    /// <returns><see langword="true"/> if the item was part of this group, otherwise <see langword="false"/></returns>
    public bool Remove(RadioItem item)
    {
        if (!ReferenceEquals(item.Group, this)) return false;

        item.Group = null;
        return true;
    }

    /// <summary>
    /// Clears the selection so that no item is selected
    /// </summary>
    public void ClearSelection()
    {
        RadioItem? previous;

        lock (_syncRoot)
        {
            previous = selected;
            selected = null;
        }

        if (previous is null) return;

        previous.ApplyChecked(false);
        SelectionChanged?.Invoke(null);
    }

    internal void Attach(RadioItem item)
    {
        lock (_syncRoot)
        {
            if (!_items.Contains(item)) _items.Add(item);
        }

        if (item.IsChecked) Select(item);
    }

    internal void Detach(RadioItem item)
    {
        bool wasSelected;

        lock (_syncRoot)
        {
            _items.Remove(item);

            wasSelected = ReferenceEquals(selected, item);
            if (wasSelected) selected = null;
        }

        if (!wasSelected) return;

        item.ApplyChecked(false);
        SelectionChanged?.Invoke(null);
    }

    internal void Select(RadioItem item)
    {
        RadioItem? previous;

        lock (_syncRoot)
        {
            if (!_items.Contains(item) || ReferenceEquals(selected, item)) return;

            previous = selected;
            selected = item;
        }

        previous?.ApplyChecked(false);
        item.ApplyChecked(true);

        SelectionChanged?.Invoke(item);
    }

    internal void Deselect(RadioItem item)
    {
        lock (_syncRoot)
        {
            if (!ReferenceEquals(selected, item)) return;

            selected = null;
        }

        item.ApplyChecked(false);
        SelectionChanged?.Invoke(null);
    }
}