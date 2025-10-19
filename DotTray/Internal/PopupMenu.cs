namespace DotTray.Internal;

using DotTray.Internal.Native;
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
    private readonly PInvoke.WndProc _wndProcFunc;
    private readonly AutoResetEvent _closed;

    private bool disposed;
    private int hoverIndex;

    private PopupMenu(nint ownerHWnd, NotifyIcon icon, MenuItemCollection items, POINT screenPos, string windowClassName)
    {
        _windowClassName = windowClassName;
        _closed = new AutoResetEvent(false);

        _ownerIcon = icon;
        _ownerHWnd = ownerHWnd;
        _items = items;
        _instanceHandle = PInvoke.GetModuleHandle(null);
        hoverIndex = -1;

        _wndProcFunc = WndProcFunc;

        var wndClass = new WNDCLASS
        {
            style = PInvoke.CS_HREDRAW | PInvoke.CS_VREDRAW,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcFunc),
            cbClsExtra = 0,
            cbWndExtra = 0,
            hInstance = _instanceHandle,
            hIcon = nint.Zero,
            hCursor = nint.Zero,
            hbrBackground = PInvoke.NULL_BRUSH,
            lpszMenuName = null,
            lpszClassName = _windowClassName
        };
        _ = PInvoke.RegisterClass(ref wndClass);

        var hdc = PInvoke.GetDC(nint.Zero);
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

                    _ = PInvoke.DrawText(hdc, menuItem.Text, -1, ref rect, 0x0400);

                    var itemTotal = rect.Right - rect.Left + 44 + (menuItem.SubMenu.Count > 0 ? 12 : 0);
                    if (itemTotal > maxWidth) maxWidth = itemTotal;
                }
            }
        }
        finally
        {
            _ = PInvoke.ReleaseDC(nint.Zero, hdc);
        }

        var maxHeight = items.Count * ItemHeight;

        var screenWidth = PInvoke.GetSystemMetrics(PInvoke.SM_CXSCREEN);
        var screenHeight = PInvoke.GetSystemMetrics(PInvoke.SM_CYSCREEN);

        var x = screenPos.x;
        var y = screenPos.y;

        if (x + maxWidth > screenWidth) x = screenWidth - maxWidth - 1;
        if (y + maxHeight > screenHeight) y = screenHeight - maxHeight - 1;

        if (x < 0) x = 0;
        if (y < 0) y = 0;

        _hWnd = PInvoke.CreateWindowEx(
            PInvoke.WS_EX_TOOLWINDOW | PInvoke.WS_EX_NOACTIVATE,
            _windowClassName,
            "",
            PInvoke.WS_POPUP | PInvoke.WS_CLIPSIBLINGS | PInvoke.WS_CLIPCHILDREN | PInvoke.WS_BORDER,
            screenPos.x, screenPos.y, maxWidth, maxHeight,
            _ownerHWnd, nint.Zero, _instanceHandle, nint.Zero);
        
        try
        {
            var pref = PInvoke.DWMWCP_ROUND;
            _ = PInvoke.DwmSetWindowAttribute(_hWnd, PInvoke.DWMWA_WINDOW_CORNER_PREFERENCE, ref pref, sizeof(int));

            var backdrop = PInvoke.DWMSBT_TRANSIENTWINDOW;
            _ = PInvoke.DwmSetWindowAttribute(_hWnd, PInvoke.DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int));
        }
        catch (Exception) { }

        PInvoke.ShowWindow(_hWnd, PInvoke.SW_SHOWNOACTIVATE);
        PInvoke.UpdateWindow(_hWnd);

        PInvoke.SetWindowPos(
            _hWnd,
            PInvoke.HWND_TOPMOST,
            0, 0, 0, 0,
            PInvoke.SWP_NOMOVE | PInvoke.SWP_NOSIZE | PInvoke.SWP_NOACTIVATE);

        _ = PInvoke.SetCapture(_hWnd);

        while (!_closed.WaitOne(0))
        {
            while (PInvoke.PeekMessage(out var msg, nint.Zero, 0, 0, 1))
            {
                if (msg.message == PInvoke.WM_QUIT) return;

                PInvoke.TranslateMessage(ref msg);
                PInvoke.DispatchMessage(ref msg);
            }
        }

        _ = PInvoke.ReleaseCapture();
        _ = PInvoke.DestroyWindow(_hWnd);
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;

        _closed.Set();
    }

    private nint WndProcFunc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case PInvoke.WM_PAINT: Paint(); return nint.Zero;

            case PInvoke.WM_KEYDOWN:
                if (wParam.ToInt32() == (int)ConsoleKey.Escape) Dispose();
                return nint.Zero;

            case PInvoke.WM_KILLFOCUS or PInvoke.WM_DESTROY: Dispose(); return nint.Zero;
        }

        return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private void Paint()
    {
        var psHdc = PInvoke.BeginPaint(_hWnd, out var ps);

        try
        {
            PInvoke.GetClientRect(_hWnd, out var rcClient);

            var memDC = PInvoke.CreateCompatibleDC(psHdc);
            var memBmp = PInvoke.CreateCompatibleBitmap(psHdc, rcClient.Right, rcClient.Bottom);
            var oldBmp = PInvoke.SelectObject(memDC, memBmp);

            var bgBrush = PInvoke.CreateSolidBrush(new Rgb(80, 80, 80));
            PInvoke.FillRect(memDC, ref rcClient, bgBrush);
            PInvoke.DeleteObject(bgBrush);

            for (int i = 0; i < _items.Count; i++)
            {
                var rcItem = new RECT
                {
                    Left = 0,
                    Top = i * ItemHeight,
                    Right = rcClient.Right,
                    Bottom = (i + 1) * ItemHeight
                };

                if (i == hoverIndex)
                {
                    var hoverBrush = PInvoke.CreateSolidBrush(new Rgb(200, 0, 0));
                    PInvoke.FillRect(memDC, ref rcItem, hoverBrush);
                    PInvoke.DeleteObject(hoverBrush);
                }

                if (_items[i] is MenuItem menuItem)
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
                        var hPen = PInvoke.CreatePen(PInvoke.PS_SOLID, 2, new Rgb(255, 255, 255));
                        var oldPen = PInvoke.SelectObject(memDC, hPen);

                        PInvoke.MoveToEx(memDC, rcCheck.Left + 2, rcCheck.Top + 8, nint.Zero);
                        PInvoke.LineTo(memDC, rcCheck.Left + 6, rcCheck.Top + 12);
                        PInvoke.LineTo(memDC, rcCheck.Left + 14, rcCheck.Top + 2);

                        PInvoke.SelectObject(memDC, oldPen);
                        PInvoke.DeleteObject(hPen);
                    }

                    int textLeft = rcCheck.Right + 6;
                    int textTop = rcItem.Top + 6;
                    var textColor = menuItem.IsDisabled ? new Rgb(0, 0, 0) : new Rgb(200, 200, 200);
                    _ = PInvoke.SetTextColor(memDC, textColor);
                    _ = PInvoke.SetBkMode(memDC, PInvoke.TRANSPARENT);

                    PInvoke.TextOut(memDC, textLeft, textTop + (ItemHeight - 16) / 2, menuItem.Text, menuItem.Text.Length);

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

                        var arrowBrush = PInvoke.CreateSolidBrush(textColor);
                        var oldBrush = PInvoke.SelectObject(memDC, arrowBrush);

                        unsafe
                        {
                            fixed (POINT* ptr = pts)
                            {
                                PInvoke.Polygon(memDC, ptr, pts.Length);
                            }
                        }

                        PInvoke.SelectObject(memDC, oldBrush);
                        PInvoke.DeleteObject(arrowBrush);
                    }
                }
                else
                {
                    int sepY = (rcItem.Top + rcItem.Bottom) / 2;
                    var hPen = PInvoke.CreatePen(PInvoke.PS_SOLID, 1, new Rgb(80, 80, 80));
                    var oldPen = PInvoke.SelectObject(memDC, hPen);
                    PInvoke.MoveToEx(memDC, 8, sepY, IntPtr.Zero);
                    PInvoke.LineTo(memDC, rcClient.Right - 8, sepY);
                    PInvoke.SelectObject(memDC, oldPen);
                    PInvoke.DeleteObject(hPen);
                }
            }

            PInvoke.BitBlt(psHdc, 0, 0, rcClient.Right, rcClient.Bottom, memDC, 0, 0, PInvoke.SRCCOPY);

            PInvoke.SelectObject(memDC, oldBmp);
            PInvoke.DeleteObject(memBmp);
            PInvoke.DeleteDC(memDC);
        }
        finally
        {
            PInvoke.EndPaint(_hWnd, ref ps);
        }
    }

    public static PopupMenu Show(nint ownerHWnd, NotifyIcon icon, MenuItemCollection items, POINT screenPos, uint trayId)
        => new PopupMenu(ownerHWnd, icon, items, screenPos, $"{nameof(DotTray)}NotifyIconWindow{trayId}_Popup");
}