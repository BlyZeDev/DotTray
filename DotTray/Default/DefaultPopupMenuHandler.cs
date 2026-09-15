namespace DotTray.Default;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using DotTray.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

/// <summary>
/// The default Win32 popup behaviour
/// </summary>
public sealed class DefaultPopupMenuHandler : PopupMenuHandler, IDisposable
{
    private const uint WM_MENUREFRESH = PInvoke.WM_APP + 10;

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private volatile nint hWnd;
    private volatile bool requestedReopen;

    /// <summary>
    /// The menu items of this handler
    /// </summary>
    public ObservableCollection<ItemBase> Items { get; }

    /// <summary>
    /// Initializes the handler with an empty <see cref="Items"/> collection
    /// </summary>
    public DefaultPopupMenuHandler()
    {
        Items = [];
        Items.CollectionChanged += ItemsChanged;
    }

    /// <inheritdoc/>
    protected override void Show<THandler>(NotifyIcon<THandler> owner, Pos mousePosition) => ShowPopupMenu(owner.NativeWindowHandle, owner.InstanceHandle, mousePosition);

    /// <inheritdoc/>
    protected override void ShowContext<THandler>(NotifyIcon<THandler> owner, Pos mousePosition) => ShowPopupMenu(owner.NativeWindowHandle, owner.InstanceHandle, mousePosition);

    private void ItemsChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        foreach (var item in args.OldItems?.OfType<MenuItemBase>() ?? [])
        {
            item.Updated -= Refresh;
        }

        foreach (var item in args.NewItems?.OfType<MenuItemBase>() ?? [])
        {
            item.Updated += Refresh;
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
        var currentHWnd = hWnd;
        if (currentHWnd == nint.Zero) return;

        requestedReopen = true;
        PInvoke.PostMessage(currentHWnd, WM_MENUREFRESH, 0, 0);
    }

    private void ShowPopupMenu(nint ownerHWnd, nint instanceHandle, Pos mousePosition)
    {
        if (!_semaphore.Wait(0)) return;

        var wndProc = new PInvoke.WndProc(WndProc);
        var className = Marshal.StringToHGlobalUni($"{nameof(DefaultPopupMenuHandler)}Window{Guid.CreateVersion7()}");

        try
        {
            var wndClass = new WNDCLASS
            {
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProc),
                hInstance = instanceHandle,
                lpszClassName = className
            };
            if (PInvoke.RegisterClass(ref wndClass) == 0) return;

            hWnd = PInvoke.CreateWindowEx(0, className, nint.Zero, 0, 0, 0, 0, 0, nint.Zero, nint.Zero, instanceHandle, nint.Zero);
            if (hWnd == nint.Zero) return;

            RunMenuLoop(ownerHWnd, mousePosition);
        }
        finally
        {
            if (hWnd != nint.Zero)
            {
                PInvoke.DestroyWindow(hWnd);
                hWnd = nint.Zero;
            }

            PInvoke.UnregisterClass(className, instanceHandle);
            Marshal.FreeHGlobal(className);

            requestedReopen = false;
            _semaphore.Release();

            GC.KeepAlive(wndProc);
        }
    }

    private void RunMenuLoop(nint ownerHWnd, Pos mousePosition)
    {
        do
        {
            requestedReopen = false;

            var clicked = ShowMenu(ownerHWnd, mousePosition);
            if (clicked is not null)
            {
                if (clicked is CheckItem check) check.IsChecked = !check.IsChecked;
                clicked.RaiseClick();
                return;
            }

            if (requestedReopen)
            {
                DrainPendingMessages();
            }
        }
        while (requestedReopen);
    }

    private MenuItemBase? ShowMenu(nint ownerHWnd, Pos mousePosition)
    {
        var createdMenus = new List<nint>();
        var itemsById = new Dictionary<nuint, MenuItemBase>();

        try
        {
            var nextId = 1u;
            var hMenu = BuildMenu(Items, createdMenus, itemsById, ref nextId);
            if (hMenu == nint.Zero) return null;

            TryEnableDarkMode(ownerHWnd);
            PInvoke.SetForegroundWindow(ownerHWnd);

            var result = PInvoke.TrackPopupMenuEx(
                hMenu,
                PInvoke.TPM_LEFTALIGN | PInvoke.TPM_BOTTOMALIGN | PInvoke.TPM_RETURNCMD | PInvoke.TPM_NONOTIFY,
                mousePosition.X,
                mousePosition.Y,
                ownerHWnd,
                nint.Zero);

            PInvoke.PostMessage(ownerHWnd, PInvoke.WM_NULL, 0, 0);

            return result != 0 && itemsById.TryGetValue((nuint)result, out var clicked) ? clicked : null;
        }
        finally
        {
            foreach (var menu in createdMenus)
            {
                PInvoke.DestroyMenu(menu);
            }
        }
    }

    private static nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        if (msg == WM_MENUREFRESH)
        {
            PInvoke.EndMenu();
            return 0;
        }

        return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private static nint BuildMenu(IEnumerable<ItemBase> items, List<nint> createdMenus, Dictionary<nuint, MenuItemBase> itemsById, ref uint nextId)
    {
        var hMenu = PInvoke.CreatePopupMenu();
        if (hMenu == nint.Zero) return nint.Zero;

        createdMenus.Add(hMenu);

        foreach (var item in items)
        {
            switch (item)
            {
                case SeparatorItem:
                    PInvoke.AppendMenu(hMenu, PInvoke.MF_SEPARATOR, 0, null);
                    break;

                case SubmenuItem { Items.Count: > 0 } submenu:
                    var hSubmenu = BuildMenu(submenu.Items, createdMenus, itemsById, ref nextId);
                    if (hSubmenu == nint.Zero) break;

                    var submenuFlags = PInvoke.MF_STRING | PInvoke.MF_POPUP;
                    if (submenu.IsDisabled) submenuFlags |= PInvoke.MF_GRAYED;

                    PInvoke.AppendMenu(hMenu, submenuFlags, (nuint)hSubmenu, submenu.Text);
                    break;

                case MenuItemBase menuItem:
                    var id = nextId++;
                    itemsById[id] = menuItem;

                    var flags = PInvoke.MF_STRING;
                    if (menuItem.IsDisabled) flags |= PInvoke.MF_GRAYED;
                    if (menuItem is CheckItem { IsChecked: true }) flags |= PInvoke.MF_CHECKED;

                    PInvoke.AppendMenu(hMenu, flags, id, menuItem.Text);
                    break;
            }
        }

        return hMenu;
    }

    private static void DrainPendingMessages()
    {
        while (PInvoke.PeekMessage(out var message, nint.Zero, 0, 0, PInvoke.PM_REMOVE))
        {
            PInvoke.TranslateMessage(ref message);
            PInvoke.DispatchMessage(ref message);
        }

        Thread.Sleep(1);
    }

    private static void TryEnableDarkMode(nint hWnd)
    {
        try
        {
            var enabled = 1;
            PInvoke.DwmSetWindowAttribute(hWnd, PInvoke.DWMWA_USE_IMMERSIVE_DARK_MODE, ref enabled, sizeof(int));

            try
            {
                SetPreferredAppMode(1);
            }
            catch
            {
                AllowDarkModeForApp(true);
            }

            AllowDarkModeForWindow(hWnd, true);

            FlushMenuThemes();
        }
        catch (Exception) { }
    }

    void IDisposable.Dispose()
    {
        Items.CollectionChanged -= ItemsChanged;

        foreach (var item in Items.OfType<MenuItemBase>())
        {
            item.Updated -= Refresh;
        }

        _semaphore.Dispose();
    }
    //Experimental
    [DllImport("uxtheme.dll", EntryPoint = "#132")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AllowDarkModeForApp([MarshalAs(UnmanagedType.Bool)] bool allow);

    [DllImport("uxtheme.dll", EntryPoint = "#133")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AllowDarkModeForWindow(nint hWnd, [MarshalAs(UnmanagedType.Bool)] bool allow);

    [DllImport("uxtheme.dll", EntryPoint = "#135")]
    private static extern int SetPreferredAppMode(int appMode);

    [DllImport("uxtheme.dll", EntryPoint = "#136")]
    private static extern void FlushMenuThemes();
}