namespace DotTray.Internal;

using DotTray.Internal.Win32;
using System;
using System.Runtime.InteropServices;
using System.Threading;

internal sealed class PopupMenu : IDisposable
{
    private const int ItemHeight = 28;

    private readonly string _windowClassName;
    private readonly NotifyIcon _ownerIcon;
    private readonly nint _ownerHWnd;
    private readonly nint _hWnd;
    private readonly nint _instanceHandle;
    private readonly MenuItemCollection _items;
    private readonly Native.WndProc _wndProcFunc;
    private readonly AutoResetEvent _closed;

    private bool disposed;
    private int hoverIndex;
    private PopupMenu? subMenuPopup;

    private PopupMenu(nint ownerHWnd, NotifyIcon icon, MenuItemCollection items, POINT screenPos, string windowClassName)
    {
        _windowClassName = windowClassName;
        _closed = new AutoResetEvent(false);

        _ownerIcon = icon;
        _ownerHWnd = ownerHWnd;
        _items = items;
        _instanceHandle = Native.GetModuleHandle(null);

        _wndProcFunc = WndProcFunc;

        var wndClass = new WNDCLASS
        {
            style = Native.CS_HREDRAW | Native.CS_VREDRAW,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcFunc),
            cbClsExtra = 0,
            cbWndExtra = 0,
            hInstance = _instanceHandle,
            hIcon = nint.Zero,
            hCursor = nint.Zero,
            hbrBackground = Native.NULL_BRUSH,
            lpszMenuName = null,
            lpszClassName = _windowClassName
        };
        _ = Native.RegisterClass(ref wndClass);

        var hdc = Native.GetDC(nint.Zero);
        var maxWidth = 160;

        try
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] is MenuItem menuItem)
                {
                    var rect = new RECT
                    {
                        Left = 0,
                        Top = 0,
                        Right = 1000,
                        Bottom = ItemHeight
                    };

                    _ = Native.DrawText(hdc, menuItem.Text, -1, ref rect, 0x0400);

                    var itemTotal = rect.Right - rect.Left + 44 + (menuItem.SubMenu.Count > 0 ? 12 : 0);
                    if (itemTotal > maxWidth) maxWidth = itemTotal;
                }
            }
        }
        finally
        {
            _ = Native.ReleaseDC(nint.Zero, hdc);
        }

        _hWnd = Native.CreateWindowEx(
            Native.WS_EX_TOOLWINDOW | Native.WS_EX_NOACTIVATE,
            _windowClassName,
            "",
            Native.WS_POPUP | Native.WS_CLIPSIBLINGS | Native.WS_CLIPCHILDREN | Native.WS_BORDER,
            screenPos.x, screenPos.y, maxWidth, items.Count * ItemHeight,
            _ownerHWnd, nint.Zero, _instanceHandle, nint.Zero);

        try
        {
            var pref = Native.DWMWCP_ROUND;
            _ = Native.DwmSetWindowAttribute(_hWnd, Native.DWMWA_WINDOW_CORNER_PREFERENCE, ref pref, sizeof(int));

            var backdrop = Native.DWMSBT_TRANSIENTWINDOW;
            _ = Native.DwmSetWindowAttribute(_hWnd, Native.DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int));
        }
        catch (Exception) { }

        Native.ShowWindow(_hWnd, Native.SW_SHOWNOACTIVATE);
        Native.UpdateWindow(_hWnd);

        Native.SetWindowPos(
            _hWnd,
            Native.HWND_TOPMOST,
            0, 0, 0, 0,
            Native.SWP_NOMOVE | Native.SWP_NOSIZE | Native.SWP_NOACTIVATE);

        _ = Native.SetCapture(_hWnd);

        hoverIndex = -1;
    }

    public PopupMenu(nint ownerHWnd, NotifyIcon icon, MenuItemCollection items, POINT screenPos, uint trayId)
        : this(ownerHWnd, icon, items, screenPos, $"{nameof(DotTray)}NotifyIconWindow{trayId}_Popup") { }

    public void ShowModal()
    {
        Native.ShowWindow(_hWnd, Native.SW_SHOWNOACTIVATE);
        Native.UpdateWindow(_hWnd);

        while (!_closed.WaitOne(0))
        {
            while (Native.PeekMessage(out var msg, nint.Zero, 0, 0, 1))
            {
                if (msg.message == Native.WM_QUIT) return;

                Native.TranslateMessage(ref msg);
                Native.DispatchMessage(ref msg);
            }
        }
    }

    public void Dispose()
    {
        if (disposed) return;

        _ = Native.ReleaseCapture();
        _ = Native.DestroyWindow(_hWnd);
        _closed.Set();

        disposed = true;
    }

    private nint WndProcFunc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case Native.WM_ERASEBKGND: return 1;

            case Native.WM_PAINT: Paint(); return nint.Zero;

            case Native.WM_MOUSEMOVE: UpdateHover((short)((lParam.ToInt32() >> 16) & 0xFFFF)); return nint.Zero;
            case Native.WM_LBUTTONDOWN: HandleClick((short)((lParam.ToInt32() >> 16) & 0xFFFF)); return nint.Zero;

            case Native.WM_KEYDOWN: if (wParam.ToInt32() == (int)ConsoleKey.Escape) Dispose(); return nint.Zero;

            case Native.WM_KILLFOCUS: Dispose(); return nint.Zero;
            case Native.WM_DESTROY: return nint.Zero;
        }

        return Native.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private void UpdateHover(int mouseY)
    {
        var index = mouseY / ItemHeight;
        if (index < 0 || index >= _items.Count)
        {
            if (hoverIndex != -1)
            {
                hoverIndex = -1;
                CloseSubmenu();
                Native.InvalidateRect(_hWnd, nint.Zero, true);
            }

            return;
        }

        if (index == hoverIndex) return;

        hoverIndex = index;
        Native.InvalidateRect(_hWnd, nint.Zero, true);

        if (_items[index] is MenuItem menuItem)
        {
            if (menuItem.SubMenu.Count > 0) ShowSubmenu(index, menuItem.SubMenu);
            else CloseSubmenu();
        }
        else CloseSubmenu();
    }

    private void HandleClick(int mouseY)
    {
        var index = mouseY / ItemHeight;
        if (index < 0 || index >= _items.Count)
        {
            Dispose();
            return;
        }

        if (_items[index] is not MenuItem menuItem) return;

        if (menuItem.SubMenu.Count > 0) return;

        if (menuItem.IsChecked.HasValue) menuItem.IsChecked = !menuItem.IsChecked;

        menuItem.Clicked?.Invoke(new MenuItemClickedArgs
        {
            Icon = _ownerIcon,
            MenuItem = menuItem
        });

        Dispose();
    }

    private void ShowSubmenu(int index, MenuItemCollection subMenu)
    {
        if (subMenuPopup is not null) return;

        CloseSubmenu();

        Native.GetClientRect(_hWnd, out var rect);

        var pos = new POINT
        {
            x = rect.Right,
            y = index * ItemHeight
        };
        Native.ClientToScreen(_hWnd, ref pos);

        subMenuPopup = new PopupMenu(_ownerHWnd, _ownerIcon, subMenu, pos, unchecked((uint)Environment.TickCount));
        subMenuPopup.ShowModal();
    }

    private void CloseSubmenu()
    {
        if (subMenuPopup is null) return;

        subMenuPopup.Dispose();
        subMenuPopup = null;
    }

    private void Paint()
    {
        var hdc = Native.GetDC(_hWnd);
        try
        {
            Native.GetClientRect(_hWnd, out var rcClient);

            var bgBrush = Native.CreateSolidBrush(new Rgb(80, 80, 80));
            Native.FillRect(hdc, ref rcClient, bgBrush);
            Native.DeleteObject(bgBrush);

            for (int i = 0; i < _items.Count; i++)
            {
                var entry = _items[i];
                var rcItem = new RECT
                {
                    Left = 0,
                    Top = i * ItemHeight,
                    Right = rcClient.Right,
                    Bottom = (i + 1) * ItemHeight
                };

                if (i == hoverIndex)
                {
                    var hoverBrush = Native.CreateSolidBrush(new Rgb(200, 0, 0));
                    Native.FillRect(hdc, ref rcItem, hoverBrush);
                    Native.DeleteObject(hoverBrush);
                }

                if (entry is MenuItem menuItem)
                {
                    RECT rcCheck = new()
                    {
                        Left = 4,
                        Top = rcItem.Top + (ItemHeight - 16) / 2,
                        Right = 20,
                        Bottom = rcItem.Top + (ItemHeight + 16) / 2
                    };

                    if (menuItem.IsChecked.GetValueOrDefault())
                    {
                        var hPen = Native.CreatePen(Native.PS_SOLID, 2, new Rgb(255, 255, 255));
                        var oldPen = Native.SelectObject(hdc, hPen);

                        Native.MoveToEx(hdc, rcCheck.Left + 2, rcCheck.Top + 8, nint.Zero);
                        Native.LineTo(hdc, rcCheck.Left + 6, rcCheck.Top + 12);
                        Native.LineTo(hdc, rcCheck.Left + 14, rcCheck.Top + 2);

                        Native.SelectObject(hdc, oldPen);
                        Native.DeleteObject(hPen);
                    }

                    int textLeft = rcCheck.Right + 6;
                    int textTop = rcItem.Top + 6;
                    var textColor = menuItem.IsDisabled ? new Rgb(0, 0, 0) : new Rgb(200, 200, 200);
                    _ = Native.SetTextColor(hdc, textColor);
                    _ = Native.SetBkMode(hdc, Native.TRANSPARENT);

                    Native.TextOut(hdc, textLeft, textTop + (ItemHeight - 16) / 2, menuItem.Text, menuItem.Text.Length);

                    if (menuItem.SubMenu.Count > 0)
                    {
                        int arrowX = rcClient.Right - 12;
                        int arrowY = rcItem.Top + (ItemHeight / 2);

                        Span<POINT> pts =
                        [
                            new POINT{ x = arrowX, y = arrowY - 4 },
                            new POINT{ x = arrowX + 6, y = arrowY },
                            new POINT{ x = arrowX, y = arrowY + 4 }
                        ];

                        var arrowBrush = Native.CreateSolidBrush(textColor);
                        var oldBrush = Native.SelectObject(hdc, arrowBrush);

                        unsafe
                        {
                            fixed (POINT* ptr = pts)
                            {
                                Native.Polygon(hdc, ptr, pts.Length);
                            }
                        }
                        
                        Native.SelectObject(hdc, oldBrush);
                        Native.DeleteObject(arrowBrush);
                    }
                }
                else
                {
                    int sepY = (rcItem.Top + rcItem.Bottom) / 2;
                    var hPen = Native.CreatePen(Native.PS_SOLID, 1, new Rgb(80, 80, 80));
                    var oldPen = Native.SelectObject(hdc, hPen);
                    Native.MoveToEx(hdc, 8, sepY, IntPtr.Zero);
                    Native.LineTo(hdc, rcClient.Right - 8, sepY);
                    Native.SelectObject(hdc, oldPen);
                    Native.DeleteObject(hPen);
                }
            }
        }
        finally
        {
            _ = Native.ReleaseDC(_hWnd, hdc);
        }
    }
}