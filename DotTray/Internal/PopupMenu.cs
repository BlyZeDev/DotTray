namespace DotTray.Internal;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using DotTray.Popup.Default;
using DotTray.Primitives;
using System;
using System.Runtime.InteropServices;

internal sealed class PopupMenu
{
    private const float BaseDpi = 96f;
    public const uint WM_APP_POPUP_CALCWND = PInvoke.WM_APP + 0x2000;

    private readonly float _scale;
    private readonly PInvoke.WndProc _wndProc;
    private readonly PopupMenuTree _tree;

    public nint HWnd { get; }

    public PopupMenu(PopupMenuTree tree, nint ownerHWnd)
    {
        _tree = tree;
        _tree.Owner.Handler.MenuItems.Updated += RequestRedraw;

        HWnd = PInvoke.CreateWindowEx(
            PInvoke.WS_EX_NOACTIVATE | PInvoke.WS_EX_TOOLWINDOW | PInvoke.WS_EX_TOPMOST,
            _tree.Owner.PopupWindowClassName, nint.Zero,
            PInvoke.WS_CLIPCHILDREN | PInvoke.WS_CLIPSIBLINGS | PInvoke.WS_POPUP,
            0, 0, 0, 0,
            ownerHWnd,
            nint.Zero,
            _tree.Owner.InstanceHandle,
            nint.Zero);

        _scale = PInvoke.GetDpiForWindow(HWnd) / BaseDpi;

        var cornerRadius = PInvoke.DWMWCP_ROUND;
        PInvoke.DwmSetWindowAttribute(HWnd, PInvoke.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerRadius, sizeof(int));

        _wndProc = new PInvoke.WndProc(WndProcFunc);
        PInvoke.SetWindowLongPtr(HWnd, PInvoke.GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_wndProc));

        HandleCalcWnd(HWnd);
        PInvoke.ShowWindow(HWnd, PInvoke.SW_SHOWNOACTIVATE);
    }

    private void RequestRedraw() => PInvoke.PostMessage(HWnd, WM_APP_POPUP_CALCWND, nint.Zero, nint.Zero);

    private nint WndProcFunc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case PInvoke.WM_NCACTIVATE: return 1;
            case WM_APP_POPUP_CALCWND: return HandleCalcWnd(hWnd);
            case PInvoke.WM_SIZE: PInvoke.InvalidateRect(hWnd, nint.Zero, false); return 0;
            case PInvoke.WM_ERASEBKGND: return 1;
            case PInvoke.WM_PAINT: return HandlePaint(hWnd);

            case PInvoke.WM_CLOSE: PInvoke.DestroyWindow(hWnd); return 0;
            case PInvoke.WM_DESTROY: return HandleDestroy();
        }

        return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private nint HandleCalcWnd(nint hWnd)
    {
        PInvoke.GetCursorPos(out var pos);
        var (x, y, width, height) = CalcWindowArea(pos, _tree.Owner.Handler.MenuItems);
        PInvoke.SetWindowPos(hWnd, nint.Zero, x, y, width, height, PInvoke.SWP_ZORDER | PInvoke.SWP_NOACTIVATE);

        return nint.Zero;
    }

    private nint HandlePaint(nint hWnd)
    {
        var hPaint = PInvoke.BeginPaint(hWnd, out var paint);

        try
        {
            PInvoke.GetClientRect(hWnd, out var cRect);
            var bounds = new Rectangle
            {
                X = cRect.Left,
                Y = cRect.Top,
                Width = cRect.Right - cRect.Left,
                Height = cRect.Bottom - cRect.Top
            };

            var dc = PInvoke.CreateCompatibleDC(hPaint);
            var hBitmap = PInvoke.CreateCompatibleBitmap(hPaint, bounds.Width, bounds.Height);
            var hOldBitmap = PInvoke.SelectObject(dc, hBitmap);

            PInvoke.GdipCreateFromHDC(dc, out var gdip);

            using (var hBackground = _tree.Owner.Handler.Color.CreateNativeHandle(bounds))
            {
                PInvoke.GdipFillRectangleI(gdip, hBackground.DangerousGetHandle(), bounds.X, bounds.Y, bounds.Width, bounds.Height);
            }

            using (var drawing = new DrawingContext(gdip, _scale, bounds))
            {
                var itemTop = bounds.Top;
                for (int i = 0; i < _tree.Owner.Handler.MenuItems.Count; i++)
                {
                    var box = _tree.Owner.Handler.MenuItems[i].DrawBox;

                    drawing.ItemBounds = new Rectangle(bounds.Left, itemTop, box.Width, box.Height);
                    _tree.Owner.Handler.MenuItems[i].Draw(drawing);

                    itemTop += box.Height;
                }
            }

            PInvoke.GdipDeleteGraphics(gdip);

            PInvoke.BitBlt(hPaint, 0, 0, bounds.Width, bounds.Height, dc, 0, 0, PInvoke.SRCCOPY);

            PInvoke.SelectObject(dc, hOldBitmap);
            PInvoke.DeleteObject(hBitmap);
            PInvoke.DeleteDC(dc);
        }
        finally
        {
            PInvoke.EndPaint(hWnd, ref paint);
        }

        return 0;
    }

    private nint HandleDestroy()
    {
        _tree.Owner.Handler.MenuItems.Updated -= RequestRedraw;
        return nint.Zero;
    }

    private (int X, int Y, int Width, int Height) CalcWindowArea(POINT anchor, MenuItemCollection items)
    {
        var hdc = PInvoke.CreateCompatibleDC(nint.Zero);
        _ = PInvoke.GdipCreateFromHDC(hdc, out var gdip);

        var maxWidth = 0;
        var totalHeight = 0;

        using (var measuring = new MeasuringContext(gdip, _scale))
        {
            foreach (var item in items)
            {
                item.DrawBox = item.Measure(measuring);
                maxWidth = Math.Max(maxWidth, item.DrawBox.Width);
                totalHeight += item.DrawBox.Height;
            }
        }

        _ = PInvoke.GdipDeleteGraphics(gdip);
        _ = PInvoke.DeleteDC(hdc);

        var hMonitor = PInvoke.MonitorFromPoint(anchor, PInvoke.MONITOR_DEFAULTTONEAREST);
        var (screenWidth, screenHeight) = GetMonitorWorkArea(hMonitor);

        var x = anchor.x;
        var y = anchor.y;

        if (x + maxWidth > screenWidth) x = Math.Abs(x - maxWidth);
        if (y + totalHeight > screenHeight) y = Math.Abs(y - totalHeight);

        return (x, y, maxWidth, totalHeight);
    }

    private static (int Width, int Height) GetMonitorWorkArea(nint monitorHandle)
    {
        var monitorInfo = new MONITORINFO
        {
            cbSize = (uint)Marshal.SizeOf<MONITORINFO>()
        };
        PInvoke.GetMonitorInfo(monitorHandle, ref monitorInfo);

        return (monitorInfo.rcWork.Right - monitorInfo.rcWork.Left, monitorInfo.rcWork.Bottom - monitorInfo.rcWork.Top);
    }
}