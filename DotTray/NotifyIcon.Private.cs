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
                    _menuActions[id] = () => menuItem.Click?.Invoke(menuItem, this);

                    var flags = Native.MF_STRING;
                    if (menuItem.IsDisabled) flags |= Native.MF_GRAYED;
                    if (menuItem.IsChecked ?? false) flags |= Native.MF_CHECKED;

                    Native.AppendMenu(menuHandle, flags, id, menuItem.Text);
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

    private void SetIcon(nint icoHandle, bool needsIcoDestroy) => Native.PostMessage(hWnd, Native.ID_TRAY_ICON, icoHandle, needsIcoDestroy ? 1 : 0);

    private unsafe nint WndProcFunc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case Native.WM_APP_TRAYICON:
                {
                    var eventCode = (int)lParam;

                    if (eventCode is Native.WM_LBUTTONUP or Native.WM_RBUTTONUP)
                    {
                        Native.SetForegroundWindow(hWnd);
                        Native.GetCursorPos(out var point);
                        Native.TrackPopupMenu(trayMenu, Native.TPM_RIGHTBUTTON, point.x, point.y, 0, hWnd, 0);
                    }
                }
                break;

            case Native.WM_COMMAND:
                {
                    var command = (int)(wParam & 0xFFFF);
                    if (_menuActions.TryGetValue(command, out var action)) action();
                }
                break;

            case Native.ID_TRAY_ICON:
                {
                    var newIco = wParam;
                    var newNeedsIcoDestroy = lParam != 0;

                    var iconData = new NOTIFYICONDATA
                    {
                        cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                        hWnd = hWnd,
                        uID = Native.ID_TRAY_ICON,
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
                        uID = Native.ID_TRAY_ICON,
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
                        uID = Native.ID_TRAY_ICON,
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

        var handle = Native.LoadImage(nint.Zero, icoPath, Native.IMAGE_ICON, 0, 0, Native.LR_LOADFROMFILE);
        return handle == nint.Zero ? throw new FileLoadException("The .ico file could not be loaded", icoPath) : handle;
    }
}