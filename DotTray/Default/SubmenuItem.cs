namespace DotTray.Default;

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

/// <summary>
/// Represents a default Win32 menu item that can contain a submenu
/// </summary>
public sealed class SubmenuItem : MenuItem
{
    /// <summary>
    /// The submenu items of this instance
    /// </summary>
    public ObservableCollection<Win32Item> Items { get; }

    /// <summary>
    /// Initializes a <see cref="SubmenuItem"/> instance with the default configuration
    /// </summary>
    public SubmenuItem() : base()
    {
        Items = [];
        Items.CollectionChanged += ItemsChanged;
    }

    private void ItemsChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (args.OldItems is not null)
        {
            foreach (var item in args.OldItems.OfType<MenuItemBase>())
            {
                item.Updated -= Update;
            }
        }

        if (args.NewItems is not null)
        {
            foreach (var item in args.NewItems.OfType<MenuItemBase>())
            {
                item.Updated += Update;
            }
        }

        if (args.Action is NotifyCollectionChangedAction.Reset && sender is IEnumerable items)
        {
            foreach (var item in items.OfType<MenuItemBase>())
            {
                item.Updated -= Update;
                item.Updated += Update;
            }
        }
    }

    /// <summary>
    /// Cleans up resources of this instance when destroyed
    /// </summary>
    ~SubmenuItem()
    {
        Items.CollectionChanged -= ItemsChanged;

        foreach (var item in Items.OfType<MenuItemBase>())
        {
            item.Updated -= Update;
        }
    }
}