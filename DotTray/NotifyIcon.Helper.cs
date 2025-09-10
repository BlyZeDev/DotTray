namespace DotTray;

using DotTray.Internal;
using DotTray.Internal.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

public sealed partial class NotifyIcon
{
    private void BuildMenu(nint menuHandle, MenuItemCollection items)
    {
        foreach (var item in items)
        {
            if (item is MenuItem menuItem)
            {
                if (menuItem.SubMenu?.Count > 0)
                {
                    var subMenu = Native.CreatePopupMenu();
                    BuildMenu(subMenu, menuItem.SubMenu);
                    Native.AppendMenu(menuHandle, Native.MF_POPUP | (menuItem.IsDisabled ? Native.MF_GRAYED : 0), subMenu, menuItem.Text);
                }
                else
                {
                    var id = nextCommandId++;
                    _menuActions[id] = () => menuItem.Click?.Invoke(new MenuItemClickedArgs
                    {
                        Icon = this,
                        MenuItem = menuItem
                    });

                    var handle = GCHandle.Alloc(menuItem, GCHandleType.Normal);

                    Native.AppendMenu(menuHandle, Native.MF_OWNERDRAW, id, null);

                    var menuItemInfo = new MENUITEMINFO
                    {
                        cbSize = (uint)Marshal.SizeOf<MENUITEMINFO>(),
                        fMask = Native.MIIM_DATA,
                        dwItemData = (nint)handle
                    };
                    Native.SetMenuItemInfo(menuHandle, (uint)id, false, ref menuItemInfo);
                }
            }
            else Native.AppendMenu(menuHandle, Native.MF_SEPARATOR, 0, null!);
        }
    }

    private void MonitorMenuItems(MenuItemCollection menuItems, bool onlyDetach = false)
    {
        var attach = !onlyDetach;

        menuItems.EntriesChanged -= OnMenuItemChange;
        if (attach) menuItems.EntriesChanged += OnMenuItemChange;

        foreach (var item in menuItems)
        {
            if (item is MenuItem menuItem)
            {
                menuItem.Changed -= OnMenuItemChange;
                if (attach) menuItem.Changed += OnMenuItemChange;

                MonitorMenuItems(menuItem.SubMenu);
            }
        }
    }

    private void OnMenuItemChange()
    {
        if (menuRefreshQueued) return;

        menuRefreshQueued = true;
        Native.PostMessage(hWnd, Native.WM_APP_TRAYICON_REBUILD, 0, 0);
    }

    private void SetIcon(nint icoHandle, bool needsIcoDestroy) => Native.PostMessage(hWnd, Native.WM_APP_TRAYICON_ICON, icoHandle, needsIcoDestroy ? 1 : 0);

    private unsafe nint WndProcFunc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case Native.WM_APP_TRAYICON:
                {
                    var clickedButton = (int)lParam switch
                    {
                        Native.WM_LBUTTONUP => MouseButton.Left,
                        Native.WM_RBUTTONUP => MouseButton.Right,
                        Native.WM_MBUTTONUP => MouseButton.Middle,
                        _ => MouseButton.None
                    };

                    if (clickedButton is not MouseButton.None && MouseButtons.HasFlag(clickedButton))
                    {
                        MenuShowing?.Invoke(clickedButton);

                        Native.SetForegroundWindow(hWnd);
                        Native.GetCursorPos(out var pos);
                        Native.TrackPopupMenu(trayMenu, Native.TPM_RIGHTBUTTON, pos.x, pos.y, 0, hWnd, 0);

                        MenuHiding?.Invoke();
                    }
                }
                break;

            case Native.WM_COMMAND:
                {
                    var command = (int)(wParam & 0xFFFF);
                    if (_menuActions.TryGetValue(command, out var action)) action();
                }
                break;

            case Native.WM_MEASUREITEM:
                {
                    var measureItemStruct = Marshal.PtrToStructure<MEASUREITEMSTRUCT>(lParam);
                    var menuItem = (MenuItem)GCHandle.FromIntPtr(measureItemStruct.itemData).Target!;

                    var hdc = Native.GetDC(nint.Zero);
                    if (hdc == nint.Zero)
                    {
                        measureItemStruct.itemWidth = 80;
                        measureItemStruct.itemHeight = 16;
                    }
                    else
                    {
                        Native.GetTextExtentPoint32(hdc, menuItem.Text, menuItem.Text.Length, out var size);
                        _ = Native.ReleaseDC(nint.Zero, hdc);

                        measureItemStruct.itemWidth = (uint)(size.cx + 24);
                        measureItemStruct.itemHeight = (uint)(size.cy + 4);
                    }
                    
                    Marshal.StructureToPtr(measureItemStruct, lParam, true);
                }
                return 1;

            case Native.WM_DRAWITEM:
                {
                    var drawItemStruct = Marshal.PtrToStructure<DRAWITEMSTRUCT>(lParam);
                    var menuItem = (MenuItem)GCHandle.FromIntPtr(drawItemStruct.itemData).Target!;

                    var hdc = drawItemStruct.hDC;
                    var rect = drawItemStruct.rcItem;

                    var backgroundColor = Native.GetSysColorBrush((drawItemStruct.itemState & Native.ODS_SELECTED) != 0 ? Native.COLOR_HIGHLIGHT : Native.COLOR_MENU);
                    Native.FillRect(hdc, ref rect, backgroundColor);

                    var textColor = Native.GetSysColor((drawItemStruct.itemState & Native.ODS_SELECTED) != 0 ? Native.COLOR_HIGHLIGHTTEXT : Native.COLOR_MENUTEXT);
                    _ = Native.SetTextColor(hdc, textColor);
                    _ = Native.SetBkMode(hdc, Native.TRANSPARENT);

                    _ = Native.DrawText(hdc, menuItem.Text, menuItem.Text.Length, ref rect, Native.DT_SINGLELINE | Native.DT_CENTER | Native.DT_LEFT);

                    if (menuItem.IsChecked ?? false)
                    {
                        var checkRect = new RECT
                        {
                            Left = rect.Left + 2,
                            Top = rect.Top + 2,
                            Right = rect.Right + 14,
                            Bottom = rect.Bottom - 2
                        };
                        _ = Native.DrawText(hdc, "✔", 1, ref checkRect, Native.DT_SINGLELINE | Native.DT_VCENTER | Native.DT_LEFT);
                    }
                }
                return 1;

            case Native.WM_DELETEITEM:
                {
                    var handle = GCHandle.FromIntPtr(Marshal.PtrToStructure<DELETEITEMSTRUCT>(lParam).itemData);
                    if (handle.IsAllocated) handle.Free();
                }
                return 1;

            case Native.WM_APP_TRAYICON_ICON:
                {
                    var newIco = wParam;
                    var newNeedsIcoDestroy = lParam != 0;

                    var iconData = new NOTIFYICONDATA
                    {
                        cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                        hWnd = hWnd,
                        uID = _trayId,
                        uFlags = Native.NIF_ICON,
                        uCallbackMessage = Native.WM_APP_TRAYICON,
                        hIcon = newIco
                    };
                    Native.Shell_NotifyIcon(Native.NIM_MODIFY, ref iconData);

                    if (needsIcoDestroy) Native.DestroyIcon(icoHandle);

                    icoHandle = newIco;
                    needsIcoDestroy = newNeedsIcoDestroy;
                }
                break;

            case Native.WM_APP_TRAYICON_TOOLTIP:
                {
                    var iconData = new NOTIFYICONDATA
                    {
                        cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                        hWnd = hWnd,
                        uID = _trayId,
                        uFlags = Native.NIF_TIP,
                        szTip = ToolTip
                    };

                    Native.Shell_NotifyIcon(Native.NIM_MODIFY, ref iconData);
                }
                break;

            case Native.WM_APP_TRAYICON_BALLOON:
                if (nextBalloon is not null)
                {
                    var iconData = new NOTIFYICONDATA
                    {
                        cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                        hWnd = hWnd,
                        uID = _trayId,
                        hIcon = (nextBalloon.Icon is BalloonNotificationIcon.User) ? icoHandle : nint.Zero,
                        uFlags = Native.NIF_INFO,
                        dwInfoFlags = (uint)nextBalloon.Icon | (nextBalloon.NoSound ? Native.NIIF_NOSOUND : 0),
                        szInfoTitle = nextBalloon.Title,
                        szInfo = nextBalloon.Message
                    };

                    nextBalloon = null;

                    Native.Shell_NotifyIcon(Native.NIM_MODIFY, ref iconData);
                }
                break;

            case Native.WM_APP_TRAYICON_REBUILD:
                {
                    menuRefreshQueued = false;

                    if (trayMenu != nint.Zero)
                    {
                        Native.DestroyMenu(trayMenu);
                        trayMenu = nint.Zero;
                    }
                    trayMenu = Native.CreatePopupMenu();

                    _menuActions.Clear();
                    _subMenus.Clear();

                    nextCommandId = 1000;
                    BuildMenu(trayMenu, MenuItems);
                }
                return nint.Zero;

            case Native.WM_APP_TRAYICON_QUIT: Native.PostQuitMessage(0); break;
        }

        return Native.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private static NotifyIcon Run(nint iconHandle, bool needIconDestroy, MenuItemCollection menuItems, CancellationToken cancellationToken)
    {
        using (var manualLock = new ManualResetEventSlim(false))
        {
            var icon = new NotifyIcon(iconHandle, needIconDestroy, menuItems, manualLock.Set, cancellationToken);

            manualLock.Wait(cancellationToken);

            return icon;
        }
    }

    private static async Task<NotifyIcon> RunAsync(nint iconHandle, bool needIconDestroy, MenuItemCollection menuItems, CancellationToken cancellationToken)
    {
        var manualLock = new AsyncManualResetEvent(false);

        var icon = new NotifyIcon(iconHandle, needIconDestroy, menuItems, manualLock.Set, cancellationToken);

        await manualLock.WaitAsync(cancellationToken);

        return icon;
    }

    private static nint PrepareIconHandle(string icoPath)
    {
        if (!Path.GetExtension(icoPath).Equals(".ico", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("The path needs to point to an .ico file", nameof(icoPath));
        if (!File.Exists(icoPath)) throw new FileNotFoundException("The .ico file could not be found", icoPath);

        var handle = Native.LoadImage(nint.Zero, icoPath, Native.IMAGE_ICON, 16, 16, Native.LR_LOADFROMFILE);
        return handle == nint.Zero ? throw new FileLoadException("The .ico file could not be loaded", icoPath) : handle;
    }
}