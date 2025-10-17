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
                var subMenu = nint.Zero;

                if (menuItem.SubMenu.Count > 0)
                {
                    subMenu = Native.CreatePopupMenu();
                    BuildMenu(subMenu, menuItem.SubMenu);
                }

                var id = nextCommandId++;
                _menuActions[id] = () =>
                {
                    if (menuItem.IsChecked.HasValue)
                    {
                        menuItem.IsChecked = !menuItem.IsChecked;
                    }

                    menuItem.Clicked?.Invoke(new MenuItemClickedArgs
                    {
                        Icon = this,
                        MenuItem = menuItem
                    });
                };

                var handle = GCHandle.Alloc(menuItem, GCHandleType.Normal);

                Native.AppendMenu(menuHandle, Native.MF_OWNERDRAW, id, null);

                var menuItemInfo = new MENUITEMINFO
                {
                    cbSize = (uint)Marshal.SizeOf<MENUITEMINFO>(),
                    fMask = Native.MIIM_DATA | Native.MIIM_SUBMENU | Native.MIIM_STATE,
                    dwItemData = GCHandle.ToIntPtr(handle),
                    hSubMenu = subMenu,
                    fState = menuItem.fState
                };
                Native.SetMenuItemInfo(menuHandle, (uint)id, false, ref menuItemInfo);
            }
            else Native.AppendMenu(menuHandle, Native.MF_SEPARATOR, 0, null!);
        }
    }

    private void MonitorMenuItems(bool attach)
    {
        MenuItems.EntriesChanged -= OnMenuItemChange;
        if (attach) MenuItems.EntriesChanged += OnMenuItemChange;
    }

    private void OnMenuItemChange()
    {
        if (menuRebuildQueued) return;

        menuRebuildQueued = true;
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

                        /*
                        Native.SetForegroundWindow(hWnd);
                        Native.GetCursorPos(out var pos);
                        Native.TrackPopupMenu(trayMenuHWnd, Native.TPM_RIGHTBUTTON, pos.x, pos.y, 0, hWnd, 0);
                        */

                        Native.GetCursorPos(out var pos);
                        using (var popup = new PopupMenu(hWnd,this, MenuItems, pos, _trayId))
                        {
                            popup.ShowModal();
                        }

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

                    if (measureItemStruct.itemData == nint.Zero)
                    {
                        measureItemStruct.itemWidth = 80;
                        measureItemStruct.itemHeight = 16;
                    }
                    else
                    {
                        var hdc = Native.GetDC(nint.Zero);
                        if (hdc == nint.Zero)
                        {
                            measureItemStruct.itemWidth = 80;
                            measureItemStruct.itemHeight = 16;
                        }
                        else
                        {
                            var menuItem = (MenuItem)GCHandle.FromIntPtr(measureItemStruct.itemData).Target!;

                            Native.GetTextExtentPoint32(hdc, menuItem.Text, menuItem.Text.Length, out var size);
                            _ = Native.ReleaseDC(nint.Zero, hdc);

                            var extra = CHECKBOX_AREA + MENU_PADDING_X * 2;
                            if (menuItem.SubMenu.Count > 0) extra += ARROW_AREA;

                            measureItemStruct.itemWidth = (uint)(size.cx + extra);
                            measureItemStruct.itemHeight = (uint)(size.cy + MENU_PADDING_Y * 2);
                        }
                    }

                    Marshal.StructureToPtr(measureItemStruct, lParam, true);

                    return 1;
                }

            case Native.WM_DRAWITEM:
                {
                    var drawItemStruct = Marshal.PtrToStructure<DRAWITEMSTRUCT>(lParam);

                    if (drawItemStruct.itemData == nint.Zero) return 1;

                    var menuItem = (MenuItem)GCHandle.FromIntPtr(drawItemStruct.itemData).Target!;

                    var hdc = drawItemStruct.hDC;
                    var rect = drawItemStruct.rcItem;

                    var isDisabled = (drawItemStruct.itemState & Native.ODS_DISABLED) != 0;
                    var isSelected = (drawItemStruct.itemState & Native.ODS_SELECTED) != 0;

                    int textColor;
                    nint brushHandle;
                    if (isDisabled)
                    {
                        brushHandle = Native.CreateSolidBrush(menuItem.BackgroundDisabledColor);
                        textColor = menuItem.TextDisabledColor;
                    }
                    else
                    {
                        if (isSelected)
                        {
                            brushHandle = Native.CreateSolidBrush(menuItem.BackgroundHoverColor);
                            textColor = menuItem.TextHoverColor;
                        }
                        else
                        {
                            brushHandle = Native.CreateSolidBrush(menuItem.BackgroundColor);
                            textColor = menuItem.TextColor;
                        }
                    }

                    var oldBrush = Native.SelectObject(hdc, brushHandle);
                    var oldPen = Native.SelectObject(hdc, Native.GetStockObject(Native.NULL_PEN));

                    Native.RoundRect(hdc, rect.Left, rect.Top, rect.Right, rect.Bottom, 0, 0);

                    Native.SelectObject(hdc, oldBrush);
                    Native.SelectObject(hdc, oldPen);
                    Native.DeleteObject(brushHandle);

                    _ = Native.SetTextColor(hdc, textColor);
                    _ = Native.SetBkMode(hdc, Native.TRANSPARENT);

                    if ((drawItemStruct.itemState & Native.ODS_CHECKED) != 0)
                    {
                        var checkX = rect.Left + 4;
                        var checkY = rect.Top + (rect.Bottom - rect.Top - 16) / 2;

                        if (gdipToken == nint.Zero)
                        {
                            var pen = Native.CreatePen(Native.PS_SOLID, 2, new Rgb(255, 255, 255));
                            oldPen = Native.SelectObject(hdc, pen);
                            oldBrush = Native.SelectObject(hdc, Native.GetStockObject(Native.NULL_BRUSH));

                            const int PointsLength = 4;
                            var points = stackalloc POINT[PointsLength]
                            {
                                new POINT { x = checkX + 2,  y = checkY + 8  },
                                new POINT { x = checkX + 6,  y = checkY + 12 },
                                new POINT { x = checkX + 14, y = checkY + 2  },
                                new POINT { x = checkX + 14, y = checkY + 2  }
                            };

                            Native.Polyline(hdc, points, PointsLength);

                            Native.SelectObject(hdc, oldPen);
                            Native.SelectObject(hdc, oldBrush);
                            Native.DeleteObject(pen);
                        }
                        else
                        {
                            const int PointsLength = 3;
                            var points = stackalloc POINT[PointsLength]
                            {
                                new POINT { x = checkX + 2,  y = checkY + 8  },
                                new POINT { x = checkX + 6,  y = checkY + 12 },
                                new POINT { x = checkX + 14, y = checkY + 2  }
                            };

                            _ = Native.GdipCreateFromHDC(hdc, out var graphicsHandle);
                            _ = Native.GdipSetSmoothingMode(graphicsHandle, 4);
                            _ = Native.GdipCreatePen1(0xFFFFFFFF, 2f, 2, out var pen);

                            _ = Native.GdipSetPenLineJoin(pen, 2);
                            _ = Native.GdipSetPenStartCap(pen, 2);
                            _ = Native.GdipSetPenEndCap(pen, 2);

                            _ = Native.GdipDrawLinesI(graphicsHandle, pen, points, PointsLength);

                            _ = Native.GdipDeletePen(pen);
                            _ = Native.GdipDeleteGraphics(graphicsHandle);
                        }
                    }

                    rect.Left += CHECKBOX_AREA + 5;

                    _ = Native.DrawText(hdc, menuItem.Text, -1, ref rect, Native.DT_SINGLELINE | Native.DT_VCENTER | Native.DT_NOPREFIX | Native.DT_END_ELLIPSIS);

                    return 1;
                }

            case Native.WM_DELETEITEM:
                {
                    var deleteItemStruct = Marshal.PtrToStructure<DELETEITEMSTRUCT>(lParam);

                    if (deleteItemStruct.itemData != nint.Zero)
                    {
                        var handle = GCHandle.FromIntPtr(deleteItemStruct.itemData);
                        if (handle.IsAllocated) handle.Free();
                    }

                    return 1;
                }

            case Native.WM_APP_TRAYICON_ICON:
                {
                    var iconData = new NOTIFYICONDATA
                    {
                        cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                        hWnd = hWnd,
                        uID = _trayId,
                        uFlags = Native.NIF_ICON,
                        uCallbackMessage = Native.WM_APP_TRAYICON,
                        hIcon = wParam
                    };
                    Native.Shell_NotifyIcon(Native.NIM_MODIFY, ref iconData);

                    if (needsIcoDestroy) Native.DestroyIcon(icoHandle);

                    icoHandle = wParam;
                    needsIcoDestroy = lParam != 0;
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
                    menuRebuildQueued = false;

                    if (trayMenuHWnd != nint.Zero)
                    {
                        Native.DestroyMenu(trayMenuHWnd);
                        trayMenuHWnd = nint.Zero;
                    }
                    trayMenuHWnd = Native.CreatePopupMenu();

                    _menuActions.Clear();

                    nextCommandId = 1000;
                    BuildMenu(trayMenuHWnd, MenuItems);
                }
                return nint.Zero;

            case Native.WM_APP_TRAYICON_QUIT: Native.PostQuitMessage(0); break;
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
        if (!Path.GetExtension(icoPath).Equals(".ico", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("The path needs to point to an .ico file", nameof(icoPath));
        if (!File.Exists(icoPath)) throw new FileNotFoundException("The .ico file could not be found", icoPath);

        var handle = Native.LoadImage(nint.Zero, icoPath, Native.IMAGE_ICON, 16, 16, Native.LR_LOADFROMFILE);
        return handle == nint.Zero ? throw new FileLoadException("The .ico file could not be loaded", icoPath) : handle;
    }
}