namespace DotTray;

using DotTray.Internal;
using DotTray.Internal.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public sealed partial class NotifyIcon
{
    private void BuildMenu(nint menuHandle, IEnumerable<IMenuItem> items)
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

    private void MonitorMenuItems(IEnumerable<IMenuItem> menuItems, bool onlyDetach = false)
    {
        foreach (var item in menuItems)
        {
            if (item is MenuItem menuItem)
            {
                menuItem.Changed -= OnMenuItemChange;
                if (!onlyDetach) menuItem.Changed += OnMenuItemChange;

                if (menuItem.SubMenu?.Count > 0) MonitorMenuItems(menuItem.SubMenu);
            }
        }
    }

    private void ShowIcon()
    {
        var iconData = new NOTIFYICONDATA
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = hWnd,
            uID = Native.ID_TRAY_ICON,
            uFlags = Native.NIF_MESSAGE | Native.NIF_ICON,
            uCallbackMessage = Native.WM_APP_TRAYICON,
            hIcon = _iconHandle
        };

        Native.Shell_NotifyIcon(Native.NIM_ADD, ref iconData);
    }

    private void RemoveIcon()
    {
        var iconData = new NOTIFYICONDATA
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = hWnd,
            uID = Native.ID_TRAY_ICON
        };

        Native.Shell_NotifyIcon(Native.NIM_DELETE, ref iconData);
    }

    private void OnMenuItemChange()
    {
        if (menuRefreshQueued) return;

        menuRefreshQueued = true;
        Native.PostMessage(hWnd, Native.WM_APP_TRAYICON_REBUILD, 0, 0);
    }

    private unsafe nint WndProcFunc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case Native.WM_APP_TRAYICON:
                var eventCode = (int)lParam;

                if (eventCode is Native.WM_LBUTTONUP or Native.WM_RBUTTONUP)
                {
                    Native.SetForegroundWindow(hWnd);
                    Native.GetCursorPos(out var point);
                    Native.TrackPopupMenu(trayMenu, Native.TPM_RIGHTBUTTON, point.x, point.y, 0, hWnd, 0);
                }
                break;

            case Native.WM_COMMAND:
                var command = (int)(wParam & 0xFFFF);
                if (_menuActions.TryGetValue(command, out var action))
                {
                    action.Invoke();
                }
                break;

            case Native.WM_APP_TRAYICON_TOOLTIP:
                var iconData = new NOTIFYICONDATA
                {
                    cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                    hWnd = hWnd,
                    uID = Native.ID_TRAY_ICON,
                    uFlags = Native.NIF_TIP,
                };

                var tipPtr = iconData.szTip;
                for (int i = 0; i < NOTIFYICONDATA.SZTIP_BYTE_SIZE; i++)
                {
                    tipPtr[i] = 0;
                }

                var bytes = Encoding.Unicode.GetBytes(ToolTip);
                Marshal.Copy(bytes, 0, (nint)iconData.szTip, Math.Min(bytes.Length, NOTIFYICONDATA.SZTIP_BYTE_SIZE - 2));

                Native.Shell_NotifyIcon(Native.NIM_MODIFY, ref iconData);
                break;

            case Native.WM_APP_TRAYICON_REBUILD:
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

                return nint.Zero;

            case Native.WM_APP_TRAYICON_QUIT:
                Native.PostQuitMessage(0);
                break;
        }

        return Native.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private static NotifyIcon Run(nint iconHandle, bool needIconDestroy, CancellationToken cancellationToken)
    {
        using (var manualLock = new ManualResetEventSlim(false))
        {
            var icon = new NotifyIcon(iconHandle, needIconDestroy, manualLock.Set, cancellationToken);

            manualLock.Wait(cancellationToken);

            return icon;
        }
    }

    private static async Task<NotifyIcon> RunAsync(nint iconHandle, bool needIconDestroy, CancellationToken cancellationToken)
    {
        var manualLock = new AsyncManualResetEvent(false);

        var icon = new NotifyIcon(iconHandle, needIconDestroy, manualLock.Set, cancellationToken);

        await manualLock.WaitAsync(cancellationToken);

        return icon;
    }

    private static nint PrepareIconHandle(string icoPath)
    {
        if (!File.Exists(icoPath)) throw new FileNotFoundException("The .ico file could not be found", icoPath);

        var handle = Native.LoadImage(nint.Zero, icoPath, Native.IMAGE_ICON, 0, 0, Native.LR_LOADFROMFILE);
        if (handle == nint.Zero) throw new FileLoadException("The .ico file could not be loaded", icoPath);

        return handle;
    }
}