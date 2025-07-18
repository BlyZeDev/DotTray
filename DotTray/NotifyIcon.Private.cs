namespace DotTray;

using DotTray.Internal;
using DotTray.Internal.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

public sealed unsafe partial class NotifyIcon
{
    private void RebuildMenu(IEnumerable<IMenuItem> items)
    {
        if (trayMenu != nint.Zero)
        {
            Native.DestroyMenu(trayMenu);
            trayMenu = nint.Zero;
        }
        trayMenu = Native.CreatePopupMenu();

        _menuActions.Clear();
        _subMenus.Clear();

        nextCommandId = 1000;
        BuildMenu(trayMenu, items);
    }

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

    private void UpdateToolTip(string toolTip)
    {
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

        var bytes = Encoding.Unicode.GetBytes(toolTip);
        Marshal.Copy(bytes, 0, (nint)iconData.szTip, Math.Min(bytes.Length, NOTIFYICONDATA.SZTIP_BYTE_SIZE - 2));

        Native.Shell_NotifyIcon(Native.NIM_MODIFY, ref iconData);
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

    private nint WndProcFunc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case Native.WM_APP_TRAYICON:
                var eventCode = (int)lParam;

                if (eventCode is Native.WM_LBUTTONUP or Native.WM_RBUTTONUP)
                {
                    Native.SetForegroundWindow(hWnd);
                    Native.GetCursorPos(out var point);
                    Native.TrackPopupMenu(trayMenu, Native.TPM_RIGHTBUTTON | 0x0004, point.x, point.y, 0, hWnd, 0);
                }
                break;

            case Native.WM_COMMAND:
                var command = (int)(wParam & 0xFFFF);
                if (_menuActions.TryGetValue(command, out var action))
                {
                    action.Invoke();
                }
                break;

            case Native.WM_APP_TRAYICON_TOOLTIP: UpdateToolTip(ToolTip); break;

            case Native.WM_APP_TRAYICON_REBUILD:
                menuRefreshQueued = false;
                RebuildMenu(MenuItems);
                return nint.Zero;
        }

        return Native.DefWindowProc(hWnd, msg, wParam, lParam);
    }
}