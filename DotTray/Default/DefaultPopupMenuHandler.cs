namespace DotTray.Default;

using DotTray.Internal.Native;
using DotTray.Primitives;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;

/// <summary>
/// The default Win32 popup behaviour
/// </summary>
public sealed class DefaultPopupMenuHandler : PopupMenuHandler
{
    private readonly SemaphoreSlim _semaphore;

    /// <summary>
    /// The menu items of this handler
    /// </summary>
    public ObservableCollection<Win32Item> Items { get; }

    /// <summary>
    /// Initializes the handler with an empty <see cref="Items"/> collection
    /// </summary>
    public DefaultPopupMenuHandler()
    {
        _semaphore = new SemaphoreSlim(1, 1);

        Items = [];
        Items.CollectionChanged += ItemsChanged;
    }

    private void ItemsChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (args.OldItems is not null)
        {
            foreach (var item in args.OldItems.OfType<MenuItemBase>())
            {
                item.Updated -= Refresh;
            }
        }

        if (args.NewItems is not null)
        {
            foreach (var item in args.NewItems.OfType<MenuItemBase>())
            {
                item.Updated += Refresh;
            }
        }

        if (args.Action is NotifyCollectionChangedAction.Reset && sender is IEnumerable items)
        {
            foreach (var item in items.OfType<MenuItemBase>())
            {
                item.Updated -= Refresh;
                item.Updated += Refresh;
            }
        }

        Refresh();
    }

    private void Refresh()
    {

    }

    /// <inheritdoc/>
    protected override void Show<THandler>(NotifyIcon<THandler> owner, Pos mousePosition) => ShowPopupMenu(owner.NativeWindowHandle, mousePosition);

    /// <inheritdoc/>
    protected override void ShowContext<THandler>(NotifyIcon<THandler> owner, Pos mousePosition) => ShowPopupMenu(owner.NativeWindowHandle, mousePosition);

    private void ShowPopupMenu(nint ownerHWnd, Pos mousePosition)
    {
        if (!_semaphore.Wait(0)) return;

        var createdMenus = new List<nint>();
        var itemsById = new Dictionary<nuint, MenuItemBase>();

        try
        {
            var nextId = 1u;
            var hMenu = BuildMenu(Items, createdMenus, itemsById, ref nextId);
            if (hMenu == nint.Zero) return;

            PInvoke.SetForegroundWindow(ownerHWnd);

            var result = PInvoke.TrackPopupMenuEx(
                hMenu,
                PInvoke.TPM_LEFTALIGN | PInvoke.TPM_BOTTOMALIGN | PInvoke.TPM_RETURNCMD | PInvoke.TPM_NONOTIFY,
                mousePosition.X,
                mousePosition.Y,
                ownerHWnd,
                nint.Zero);

            PInvoke.PostMessage(ownerHWnd, PInvoke.WM_NULL, 0, 0);

            if (result != 0 && itemsById.TryGetValue((nuint)result, out var clicked))
            {
                if (clicked is CheckItem check) check.IsChecked = !check.IsChecked;
                clicked.RaiseClick();
            }
        }
        finally
        {
            foreach (var menu in createdMenus)
            {
                PInvoke.DestroyMenu(menu);
            }

            _semaphore.Release();
        }
    }

    private static nint BuildMenu(IEnumerable<Win32Item> items, List<nint> createdMenus, Dictionary<nuint, MenuItemBase> itemsById, ref uint nextId)
    {
        var hMenu = PInvoke.CreatePopupMenu();
        if (hMenu == nint.Zero) return nint.Zero;

        createdMenus.Add(hMenu);

        foreach (var item in items)
        {
            if (item is SeparatorItem)
            {
                PInvoke.AppendMenu(hMenu, PInvoke.MF_SEPARATOR, 0, null);
                continue;
            }

            if (item is not MenuItemBase menuItem) continue;

            if (menuItem is SubmenuItem submenu && submenu.Items.Count > 0)
            {
                var hSubmenu = BuildMenu(submenu.Items, createdMenus, itemsById, ref nextId);
                if (hSubmenu == nint.Zero) continue;

                var submenuFlags = PInvoke.MF_STRING | PInvoke.MF_POPUP;
                if (menuItem.IsDisabled) submenuFlags |= PInvoke.MF_GRAYED;

                PInvoke.AppendMenu(hMenu, submenuFlags, (nuint)hSubmenu, menuItem.Text);
            }
            else
            {
                var id = nextId++;
                itemsById[id] = menuItem;

                var flags = PInvoke.MF_STRING;

                if (menuItem.IsDisabled) flags |= PInvoke.MF_GRAYED;
                if (menuItem is CheckItem check && check.IsChecked) flags |= PInvoke.MF_CHECKED;

                PInvoke.AppendMenu(hMenu, flags, id, menuItem.Text);
            }
        }

        return hMenu;
    }

    /// <summary>
    /// Cleans up resources of this instance when destroyed
    /// </summary>
    ~DefaultPopupMenuHandler()
    {
        Items.CollectionChanged -= ItemsChanged;

        foreach (var item in Items.OfType<MenuItemBase>())
        {
            item.Updated -= Refresh;
        }

        _semaphore.Dispose();
    }
}