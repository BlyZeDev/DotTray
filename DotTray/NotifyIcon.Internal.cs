namespace DotTray;

using DotTray.Internal;
using DotTray.Internal.Native;
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
                    subMenu = PInvoke.CreatePopupMenu();
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

                PInvoke.AppendMenu(menuHandle, PInvoke.MF_OWNERDRAW, id, null);

                var menuItemInfo = new MENUITEMINFO
                {
                    cbSize = (uint)Marshal.SizeOf<MENUITEMINFO>(),
                    fMask = PInvoke.MIIM_DATA | PInvoke.MIIM_SUBMENU | PInvoke.MIIM_STATE,
                    dwItemData = GCHandle.ToIntPtr(handle),
                    hSubMenu = subMenu,
                    fState = menuItem.fState
                };
                PInvoke.SetMenuItemInfo(menuHandle, (uint)id, false, ref menuItemInfo);
            }
            else PInvoke.AppendMenu(menuHandle, PInvoke.MF_SEPARATOR, 0, null!);
        }
    }

    private void SetIcon(nint icoHandle, bool needsIcoDestroy) => PInvoke.PostMessage(hWnd, PInvoke.WM_APP_TRAYICON_ICON, icoHandle, needsIcoDestroy ? 1 : 0);

    private unsafe nint WndProcFunc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case PInvoke.WM_APP_TRAYICON:
                {
                    var clickedButton = (int)lParam switch
                    {
                        PInvoke.WM_LBUTTONUP => MouseButton.Left,
                        PInvoke.WM_RBUTTONUP => MouseButton.Right,
                        PInvoke.WM_MBUTTONUP => MouseButton.Middle,
                        _ => MouseButton.None
                    };

                    if (clickedButton is not MouseButton.None && MouseButtons.HasFlag(clickedButton))
                    {
                        MenuShowing?.Invoke(clickedButton);

                        PInvoke.GetCursorPos(out var pos);
                        using (_ = PopupMenu.Show(hWnd, this, MenuItems, pos, _trayId)) { }
                        
                        MenuHiding?.Invoke();
                    }
                }
                break;

            case PInvoke.WM_MEASUREITEM:
                {
                    var measureItemStruct = Marshal.PtrToStructure<MEASUREITEMSTRUCT>(lParam);

                    if (measureItemStruct.itemData == nint.Zero)
                    {
                        measureItemStruct.itemWidth = 80;
                        measureItemStruct.itemHeight = 16;
                    }
                    else
                    {
                        var hdc = PInvoke.GetDC(nint.Zero);
                        if (hdc == nint.Zero)
                        {
                            measureItemStruct.itemWidth = 80;
                            measureItemStruct.itemHeight = 16;
                        }
                        else
                        {
                            var menuItem = (MenuItem)GCHandle.FromIntPtr(measureItemStruct.itemData).Target!;

                            PInvoke.GetTextExtentPoint32(hdc, menuItem.Text, menuItem.Text.Length, out var size);
                            _ = PInvoke.ReleaseDC(nint.Zero, hdc);

                            var extra = CHECKBOX_AREA + MENU_PADDING_X * 2;
                            if (menuItem.SubMenu.Count > 0) extra += ARROW_AREA;

                            measureItemStruct.itemWidth = (uint)(size.cx + extra);
                            measureItemStruct.itemHeight = (uint)(size.cy + MENU_PADDING_Y * 2);
                        }
                    }

                    Marshal.StructureToPtr(measureItemStruct, lParam, true);

                    return 1;
                }

            case PInvoke.WM_DRAWITEM:
                {
                    var drawItemStruct = Marshal.PtrToStructure<DRAWITEMSTRUCT>(lParam);

                    if (drawItemStruct.itemData == nint.Zero) return 1;

                    var menuItem = (MenuItem)GCHandle.FromIntPtr(drawItemStruct.itemData).Target!;

                    var hdc = drawItemStruct.hDC;
                    var rect = drawItemStruct.rcItem;

                    var isDisabled = (drawItemStruct.itemState & PInvoke.ODS_DISABLED) != 0;
                    var isSelected = (drawItemStruct.itemState & PInvoke.ODS_SELECTED) != 0;

                    int textColor;
                    nint brushHandle;
                    if (isDisabled)
                    {
                        brushHandle = PInvoke.CreateSolidBrush(menuItem.BackgroundDisabledColor);
                        textColor = menuItem.TextDisabledColor;
                    }
                    else
                    {
                        if (isSelected)
                        {
                            brushHandle = PInvoke.CreateSolidBrush(menuItem.BackgroundHoverColor);
                            textColor = menuItem.TextHoverColor;
                        }
                        else
                        {
                            brushHandle = PInvoke.CreateSolidBrush(menuItem.BackgroundColor);
                            textColor = menuItem.TextColor;
                        }
                    }

                    var oldBrush = PInvoke.SelectObject(hdc, brushHandle);
                    var oldPen = PInvoke.SelectObject(hdc, PInvoke.GetStockObject(PInvoke.NULL_PEN));

                    PInvoke.RoundRect(hdc, rect.Left, rect.Top, rect.Right, rect.Bottom, 0, 0);

                    PInvoke.SelectObject(hdc, oldBrush);
                    PInvoke.SelectObject(hdc, oldPen);
                    PInvoke.DeleteObject(brushHandle);

                    _ = PInvoke.SetTextColor(hdc, textColor);
                    _ = PInvoke.SetBkMode(hdc, PInvoke.TRANSPARENT);

                    if ((drawItemStruct.itemState & PInvoke.ODS_CHECKED) != 0)
                    {
                        var checkX = rect.Left + 4;
                        var checkY = rect.Top + (rect.Bottom - rect.Top - 16) / 2;

                        if (gdipToken == nint.Zero)
                        {
                            var pen = PInvoke.CreatePen(PInvoke.PS_SOLID, 2, new Rgb(255, 255, 255));
                            oldPen = PInvoke.SelectObject(hdc, pen);
                            oldBrush = PInvoke.SelectObject(hdc, PInvoke.GetStockObject(PInvoke.NULL_BRUSH));

                            const int PointsLength = 4;
                            var points = stackalloc POINT[PointsLength]
                            {
                                new POINT { x = checkX + 2,  y = checkY + 8  },
                                new POINT { x = checkX + 6,  y = checkY + 12 },
                                new POINT { x = checkX + 14, y = checkY + 2  },
                                new POINT { x = checkX + 14, y = checkY + 2  }
                            };

                            PInvoke.Polyline(hdc, points, PointsLength);

                            PInvoke.SelectObject(hdc, oldPen);
                            PInvoke.SelectObject(hdc, oldBrush);
                            PInvoke.DeleteObject(pen);
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

                            _ = PInvoke.GdipCreateFromHDC(hdc, out var graphicsHandle);
                            _ = PInvoke.GdipSetSmoothingMode(graphicsHandle, 4);
                            _ = PInvoke.GdipCreatePen1(0xFFFFFFFF, 2f, 2, out var pen);

                            _ = PInvoke.GdipSetPenLineJoin(pen, 2);
                            _ = PInvoke.GdipSetPenStartCap(pen, 2);
                            _ = PInvoke.GdipSetPenEndCap(pen, 2);

                            _ = PInvoke.GdipDrawLinesI(graphicsHandle, pen, points, PointsLength);

                            _ = PInvoke.GdipDeletePen(pen);
                            _ = PInvoke.GdipDeleteGraphics(graphicsHandle);
                        }
                    }

                    rect.Left += CHECKBOX_AREA + 5;

                    _ = PInvoke.DrawText(hdc, menuItem.Text, -1, ref rect, PInvoke.DT_SINGLELINE | PInvoke.DT_VCENTER | PInvoke.DT_NOPREFIX | PInvoke.DT_END_ELLIPSIS);

                    return 1;
                }

            case PInvoke.WM_DELETEITEM:
                {
                    var deleteItemStruct = Marshal.PtrToStructure<DELETEITEMSTRUCT>(lParam);

                    if (deleteItemStruct.itemData != nint.Zero)
                    {
                        var handle = GCHandle.FromIntPtr(deleteItemStruct.itemData);
                        if (handle.IsAllocated) handle.Free();
                    }

                    return 1;
                }

            case PInvoke.WM_APP_TRAYICON_ICON:
                {
                    var iconData = new NOTIFYICONDATA
                    {
                        cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                        hWnd = hWnd,
                        uID = _trayId,
                        uFlags = PInvoke.NIF_ICON,
                        uCallbackMessage = PInvoke.WM_APP_TRAYICON,
                        hIcon = wParam
                    };
                    PInvoke.Shell_NotifyIcon(PInvoke.NIM_MODIFY, ref iconData);

                    if (needsIcoDestroy) PInvoke.DestroyIcon(icoHandle);

                    icoHandle = wParam;
                    needsIcoDestroy = lParam != 0;
                }
                break;

            case PInvoke.WM_APP_TRAYICON_TOOLTIP:
                {
                    var iconData = new NOTIFYICONDATA
                    {
                        cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                        hWnd = hWnd,
                        uID = _trayId,
                        uFlags = PInvoke.NIF_TIP,
                        szTip = ToolTip
                    };

                    PInvoke.Shell_NotifyIcon(PInvoke.NIM_MODIFY, ref iconData);
                }
                break;

            case PInvoke.WM_APP_TRAYICON_BALLOON:
                if (nextBalloon is not null)
                {
                    var iconData = new NOTIFYICONDATA
                    {
                        cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                        hWnd = hWnd,
                        uID = _trayId,
                        hIcon = (nextBalloon.Icon is BalloonNotificationIcon.User) ? icoHandle : nint.Zero,
                        uFlags = PInvoke.NIF_INFO,
                        dwInfoFlags = (uint)nextBalloon.Icon | (nextBalloon.NoSound ? PInvoke.NIIF_NOSOUND : 0),
                        szInfoTitle = nextBalloon.Title,
                        szInfo = nextBalloon.Message
                    };

                    nextBalloon = null;

                    PInvoke.Shell_NotifyIcon(PInvoke.NIM_MODIFY, ref iconData);
                }
                break;

            case PInvoke.WM_APP_TRAYICON_REBUILD:
                {
                    menuRebuildQueued = false;

                    if (trayMenuHWnd != nint.Zero)
                    {
                        PInvoke.DestroyMenu(trayMenuHWnd);
                        trayMenuHWnd = nint.Zero;
                    }
                    trayMenuHWnd = PInvoke.CreatePopupMenu();

                    nextCommandId = 1000;
                    BuildMenu(trayMenuHWnd, MenuItems);
                }
                return nint.Zero;

            case PInvoke.WM_APP_TRAYICON_QUIT: PInvoke.PostQuitMessage(0); break;
        }

        return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
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

        var handle = PInvoke.LoadImage(nint.Zero, icoPath, PInvoke.IMAGE_ICON, 16, 16, PInvoke.LR_LOADFROMFILE);
        return handle == nint.Zero ? throw new FileLoadException("The .ico file could not be loaded", icoPath) : handle;
    }
}