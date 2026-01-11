namespace DotTray.Internal;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[SupportedOSPlatform("Windows")]
internal sealed partial class PopupMenu
{
    private readonly PopupMenuLayout _layout;
    private readonly PopupMenuSession _session;
    private readonly MenuItemCollection _menuItems;
    private readonly nint _hWnd;
    private readonly PInvoke.WndProc _wndProc;

    private nint childHWnd;
    private bool isMouseTracking;
    private int hoverIndex;

    internal bool IsRoot => _session.RootHWnd == _hWnd;

    private PopupMenu(PopupMenuSession session, nint parentHWnd, MenuItemCollection menuItems, int x, int y, int width, int height)
    {
        _session = session;
        _menuItems = menuItems;

        _menuItems.Updated += OnRedrawRequired;

        hoverIndex = -1;

        _hWnd = PInvoke.CreateWindowEx(
            PInvoke.WS_EX_NOACTIVATE | PInvoke.WS_EX_TOOLWINDOW | PInvoke.WS_EX_TOPMOST,
            session.PopupWindowClassName, nint.Zero,
            PInvoke.WS_CLIPCHILDREN | PInvoke.WS_CLIPSIBLINGS | PInvoke.WS_POPUP | PInvoke.WS_VISIBLE,
            x, y, width, height,
            parentHWnd,
            nint.Zero,
            session.InstanceHandle,
            nint.Zero);

        var dpi = PInvoke.GetDpiForWindow(_hWnd);
        _layout = new PopupMenuLayout(_session.OwnerIcon.FontSize, dpi);

        _wndProc = new PInvoke.WndProc(WndProcFunc);
        PInvoke.SetWindowLongPtr(_hWnd, PInvoke.GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_wndProc));

        var cornerRadius = PInvoke.DWMWCP_ROUND;
        _ = PInvoke.DwmSetWindowAttribute(_hWnd, PInvoke.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerRadius, sizeof(int));

        var backdrop = PInvoke.DWMSBT_TRANSIENTWINDOW;
        _ = PInvoke.DwmSetWindowAttribute(_hWnd, PInvoke.DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int));

        PInvoke.ShowWindow(_hWnd, PInvoke.SW_SHOWNOACTIVATE);
        PInvoke.UpdateWindow(_hWnd);
    }

    private void OnRedrawRequired() => _session.NotifyUpdate();

    private nint WndProcFunc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case PInvoke.WM_MOUSEMOVE: return HandleMouseMove(hWnd, CheckHit(lParam));
            case PInvoke.WM_MOUSELEAVE: return HandleMouseLeave(hWnd);
            case PInvoke.WM_LBUTTONUP or PInvoke.WM_RBUTTONUP or PInvoke.WM_MBUTTONUP: return HandleMouseUp(CheckHit(lParam), msg);
            case PInvoke.WM_PAINT: return HandlePaint(hWnd);

            case PInvoke.WM_CLOSE: PInvoke.DestroyWindow(hWnd); return 0;
            case PInvoke.WM_DESTROY: return HandleDestroy();
        }

        return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
    }
    
    private nint HandleMouseMove(nint hWnd, int hitIndex)
    {
        if (!isMouseTracking)
        {
            var tme = new TRACKMOUSEEVENT
            {
                cbSize = (uint)Marshal.SizeOf<TRACKMOUSEEVENT>(),
                dwFlags = PInvoke.TME_LEAVE,
                hwndTrack = hWnd,
                dwHoverTime = 0
            };

            isMouseTracking = PInvoke.TrackMouseEvent(ref tme);
        }

        if (hitIndex == hoverIndex) return 0;

        hoverIndex = hitIndex;
        PInvoke.InvalidateRect(hWnd, nint.Zero, false);

        if (hoverIndex != -1 && _menuItems[hoverIndex] is MenuItem menuItem && !menuItem.IsDisabled)
        {
            PInvoke.SetCursor(_handCursor);
            CloseSubmenu();

            if (menuItem.HasSubMenu)
            {
                CalcWindowSize(_layout, menuItem.SubMenu, out var width, out var height);

                var topLeft = new POINT
                {
                    x = (int)MathF.Ceiling(menuItem.HitBox.X + menuItem.HitBox.Width),
                    y = (int)MathF.Ceiling(menuItem.HitBox.Y)
                };
                PInvoke.ClientToScreen(hWnd, ref topLeft);

                var x = topLeft.x;
                var y = topLeft.y;

                GetMonitorWorkArea(topLeft, out var screenWidth, out _);
                
                if (x + width > screenWidth) x = x - (int)MathF.Ceiling(menuItem.HitBox.Width) - width;
                
                childHWnd = ShowSubmenu(menuItem.SubMenu, x, y, width, height);
            }
        }
        else PInvoke.SetCursor(_arrowCursor);

        return 0;
    }

    private nint HandleMouseLeave(nint hWnd)
    {
        isMouseTracking = false;
        hoverIndex = -1;

        PInvoke.InvalidateRect(hWnd, nint.Zero, false);

        return 0;
    }

    private nint HandleMouseUp(int hitIndex, uint msg)
    {
        if (hitIndex == -1) return 0;

        if (_menuItems[hitIndex] is MenuItem menuItem && !menuItem.IsDisabled)
        {
            _menuItems.Updated -= OnRedrawRequired;

            if (menuItem.IsChecked.HasValue) menuItem.IsChecked = !menuItem.IsChecked;

            menuItem.Clicked?.Invoke(new MenuItemClickedArgs
            {
                Icon = _session.OwnerIcon,
                MenuItem = menuItem,
                MouseButton = msg switch
                {
                    PInvoke.WM_LBUTTONUP => MouseButton.Left,
                    PInvoke.WM_RBUTTONUP => MouseButton.Right,
                    PInvoke.WM_MBUTTONUP => MouseButton.Middle,
                    _ => MouseButton.None
                }
            });

            _session.Dispose();
        }

        return 0;
    }

    private nint HandleDestroy()
    {
        _menuItems.Updated -= OnRedrawRequired;

        if (IsRoot) _session.Dispose();

        return 0;
    }

    private int CheckHit(nint lParam)
    {
        var mouseX = (short)(lParam.ToInt32() & 0xFFFF);
        var mouseY = (short)((lParam.ToInt32() >> 16) & 0xFFFF);

        for (int i = 0; i < _menuItems.Count; i++)
        {
            if (_menuItems[i] is not MenuItem menuItem) continue;

            if (mouseX > menuItem.HitBox.X
                && mouseX < menuItem.HitBox.X + menuItem.HitBox.Width
                && mouseY > menuItem.HitBox.Y
                && mouseY < menuItem.HitBox.Y + menuItem.HitBox.Height) return i;
        }

        return -1;
    }

    private nint ShowSubmenu(MenuItemCollection menuItems, int x, int y, int width, int height)
    {
        var popupMenu = new PopupMenu(_session, _hWnd, menuItems, x, y, width, height);
        _session.SetLeafHWnd(popupMenu._hWnd);
        return popupMenu._hWnd;
    }

    private void CloseSubmenu()
    {
        if (childHWnd == nint.Zero) return;

        _session.SetLeafHWnd(_hWnd);

        PInvoke.PostMessage(childHWnd, PInvoke.WM_CLOSE, nint.Zero, nint.Zero);
        childHWnd = nint.Zero;
    }

    public static nint Show(PopupMenuSession session, POINT mousePos)
    {
        var monitorHandle = PInvoke.MonitorFromPoint(mousePos, PInvoke.MONITOR_DEFAULTTONEAREST);
        PInvoke.GetDpiForMonitor(monitorHandle, PInvoke.MDT_EFFECTIVE_DPI, out var dpi, out _);

        var layout = new PopupMenuLayout(session.OwnerIcon.FontSize, dpi);

        CalcWindowSize(layout, session.OwnerIcon.MenuItems, out var width, out var height);
        CalcWindowPos(mousePos, width, height, out var x, out var y);

        var menu = new PopupMenu(session, nint.Zero, session.OwnerIcon.MenuItems, x, y, width, height);
        return menu._hWnd;
    }

    private static void CalcWindowPos(POINT anchor, int width, int height, out int x, out int y)
    {
        GetMonitorWorkArea(anchor, out var screenWidth, out var screenHeight);

        x = anchor.x;
        y = anchor.y;

        if (x + width > screenWidth) x = Math.Abs(x - width);
        if (y + height > screenHeight) y = Math.Abs(y - height);
    }

    private static void CalcWindowSize(PopupMenuLayout layout, MenuItemCollection menuItems, out int width, out int height)
    {
        _ = PInvoke.GdipCreateFontFamilyFromName(FontFamilyName, nint.Zero, out var fontFamily);
        _ = PInvoke.GdipCreateFont(fontFamily, layout.FontSizePx, 0, PInvoke.UnitPixel, out var font);
        _ = PInvoke.GdipCreateStringFormat(0, 0, out var format);
        _ = PInvoke.GdipSetStringFormatFlags(format, PInvoke.StringFormatFlagsNoWrap);
        _ = PInvoke.GdipSetStringFormatAlign(format, PInvoke.StringAlignmentNear);
        _ = PInvoke.GdipSetStringFormatLineAlign(format, PInvoke.StringAlignmentCenter);

        var dcHandle = PInvoke.CreateCompatibleDC(nint.Zero);
        _ = PInvoke.GdipCreateFromHDC(dcHandle, out var graphicsHandle);

        var maxTextWidth = 0f;
        var layoutRect = new RECTF
        {
            X = 0,
            Y = 0,
            Width = float.MaxValue
        };

        string text;
        float currentHeight = 0f;
        foreach (var item in menuItems)
        {
            currentHeight += item.HeightMultiplier * layout.FontSizePx;

            if (item is MenuItem menuItem)
            {
                layoutRect.Height = item.HeightMultiplier * layout.FontSizePx;

                text = NormalizeText(menuItem.Text);
                _ = PInvoke.GdipMeasureString(graphicsHandle, text, text.Length, font, ref layoutRect, format, out var boundingBox, out _, out _);
                maxTextWidth = MathF.Max(maxTextWidth, boundingBox.Width);
            }
        }

        _ = PInvoke.GdipDeleteFont(font);
        _ = PInvoke.GdipDeleteFontFamily(fontFamily);
        _ = PInvoke.GdipDeleteGraphics(graphicsHandle);
        _ = PInvoke.DeleteDC(dcHandle);

        width = (int)MathF.Ceiling(layout.CheckBoxWidthPx + layout.TextPaddingPx + maxTextWidth + layout.TextPaddingPx + layout.SubmenuArrowWidthPx + layout.TextPaddingPx);
        height = (int)MathF.Ceiling(currentHeight);
    }

    private static string NormalizeText(string text) => text.Replace("\uFE0F", "");

    private static void GetMonitorWorkArea(POINT point, out int width, out int height)
    {
        var monitorHandle = PInvoke.MonitorFromPoint(point, PInvoke.MONITOR_DEFAULTTONEAREST);

        var monitorInfo = new MONITORINFO
        {
            cbSize = (uint)Marshal.SizeOf<MONITORINFO>()
        };
        PInvoke.GetMonitorInfo(monitorHandle, ref monitorInfo);

        width = monitorInfo.rcWork.Right - monitorInfo.rcWork.Left;
        height = monitorInfo.rcWork.Bottom - monitorInfo.rcWork.Top;
    }
}