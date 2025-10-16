namespace DotTray.Internal;

using DotTray.Internal.Win32;
using System;
using System.Runtime.InteropServices;
using System.Threading;

internal sealed class PopupMenu : IDisposable
{
    private const int ItemHeight = 28;

    private readonly string _windowClassName;
    private readonly nint _ownerHWnd;
    private readonly nint _hWnd;
    private readonly nint _instanceHandle;
    private readonly MenuItemCollection _items;
    private readonly Native.WndProc _wndProcFunc;
    private readonly AutoResetEvent _closed;

    private bool disposed;
    private int hoverIndex;

    public PopupMenu(nint ownerHWnd, MenuItemCollection items, POINT screenPos, uint trayId)
    {
        _windowClassName = $"{nameof(DotTray)}NotifyIconWindow{trayId}_Popup";
        _closed = new AutoResetEvent(false);

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
            0x80 | 0x08 | 0x08000000,
            _windowClassName,
            "",
            Native.WS_POPUP | Native.WS_CLIPSIBLINGS | Native.WS_CLIPCHILDREN,
            screenPos.x, screenPos.y, maxWidth, items.Count * ItemHeight,
            nint.Zero, nint.Zero, _instanceHandle, nint.Zero);

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

        _ = Native.SetCapture(_hWnd);

        hoverIndex = -1;
    }

    public void ShowModal() => _closed.WaitOne();

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
    }

    private nint WndProcFunc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case Native.WM_PAINT: Paint(); return nint.Zero;
            case Native.WM_KILLFOCUS: Close(); return IntPtr.Zero;
            case Native.WM_DESTROY: return IntPtr.Zero;
        }

        return Native.DefWindowProc(hWnd, msg, wParam, lParam);
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

                if (entry is MenuItem mi)
                {
                    RECT rcCheck = new()
                    {
                        Left = 4,
                        Top = rcItem.Top + (ItemHeight - 16) / 2,
                        Right = 20,
                        Bottom = rcItem.Top + (ItemHeight + 16) / 2
                    };

                    if (mi.IsChecked.HasValue && mi.IsChecked.Value)
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
                    var textColor = mi.IsDisabled ? new Rgb(0, 0, 0) : new Rgb(200, 200, 200);
                    _ = Native.SetTextColor(hdc, textColor);
                    _ = Native.SetBkMode(hdc, Native.TRANSPARENT);

                    Native.TextOut(hdc, textLeft, textTop + (ItemHeight - 16) / 2, mi.Text, mi.Text.Length);

                    if (mi.SubMenu.Count > 0)
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

    private void Close()
    {
        if (disposed) return;

        _ = Native.ReleaseCapture();
        _ = Native.DestroyWindow(_hWnd);
        _closed.Set();

        Dispose();
    }
}