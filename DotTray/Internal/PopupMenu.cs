namespace DotTray.Internal;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[SupportedOSPlatform("Windows")]
internal sealed partial class PopupMenu
{
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

        _menuItems.Updated += MenuItemUpdated;

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

        _wndProc = new PInvoke.WndProc(WndProcFunc);
        PInvoke.SetWindowLongPtr(_hWnd, PInvoke.GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_wndProc));

        var cornerRadius = PInvoke.DWMWCP_ROUND;
        _ = PInvoke.DwmSetWindowAttribute(_hWnd, PInvoke.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerRadius, sizeof(int));

        var backdrop = PInvoke.DWMSBT_TRANSIENTWINDOW;
        _ = PInvoke.DwmSetWindowAttribute(_hWnd, PInvoke.DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int));

        PInvoke.ShowWindow(_hWnd, PInvoke.SW_SHOWNOACTIVATE);
        PInvoke.UpdateWindow(_hWnd);
    }

    private void MenuItemUpdated()
    {
        PInvoke.GetWindowRect(_hWnd, out var windowRect);

        CalcWindowSize(_menuItems, out var width, out var height);
        CalcWindowPos(new POINT
        {
            x = windowRect.Left,
            y = windowRect.Top
        }, width, height, out var x, out var y);

        PInvoke.SetWindowPos(_hWnd, nint.Zero, x, y, width, height, PInvoke.SWP_NOACTIVATE | PInvoke.SWP_ZORDER);
        PInvoke.InvalidateRect(_hWnd, nint.Zero, true);
    }

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
                CalcWindowSize(menuItem.SubMenu, out var width, out var height);

                var topLeft = new POINT
                {
                    x = (int)MathF.Ceiling(menuItem.HitBox.X + menuItem.HitBox.Width),
                    y = (int)MathF.Ceiling(menuItem.HitBox.Y)
                };
                PInvoke.ClientToScreen(hWnd, ref topLeft);

                var x = topLeft.x;
                var y = topLeft.y;
                
                if (x + width > PInvoke.GetSystemMetrics(PInvoke.SM_CXSCREEN) - ScreenMargin)
                    x = x - (int)MathF.Ceiling(menuItem.HitBox.Width) - width;

                if (y + height > PInvoke.GetSystemMetrics(PInvoke.SM_CYSCREEN) - ScreenMargin)
                    y = y - (int)MathF.Ceiling(menuItem.HitBox.Height) - height;
                
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
        _menuItems.Updated -= MenuItemUpdated;

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
        CalcWindowSize(session.OwnerIcon.MenuItems, out var width, out var height);
        CalcWindowPos(mousePos, width, height, out var x, out var y);

        var menu = new PopupMenu(session, nint.Zero, session.OwnerIcon.MenuItems, x, y, width, height);
        return menu._hWnd;
    }

    private static void CalcWindowPos(POINT anchor, int width, int height, out int x, out int y)
    {
        var screenWidth = PInvoke.GetSystemMetrics(PInvoke.SM_CXSCREEN) - ScreenMargin;
        var screenHeight = PInvoke.GetSystemMetrics(PInvoke.SM_CYSCREEN) - ScreenMargin;

        x = anchor.x;
        y = anchor.y;

        if (x + width > screenWidth) x = Math.Max(ScreenMargin, Math.Abs(x - width));
        if (y + height > screenHeight) y = Math.Max(ScreenMargin, Math.Abs(y - height));
    }

    private static void CalcWindowSize(MenuItemCollection menuItems, out int width, out int height)
    {
        _ = PInvoke.GdipGetGenericFontFamilySansSerif(out var fontFamily);
        _ = PInvoke.GdipCreateFont(fontFamily, FontSize, 0, PInvoke.UnitPixel, out var font);

        var dcHandle = PInvoke.CreateCompatibleDC(nint.Zero);
        _ = PInvoke.GdipCreateFromHDC(dcHandle, out var graphicsHandle);

        var maxTextWidth = 0f;
        var layout = new RECTF
        {
            X = 0,
            Y = 0,
            Width = float.MaxValue
        };

        height = 0;
        for (int i = 0; i < menuItems.Count; i++)
        {
            if (menuItems[i] is MenuItem menuItem)
            {
                layout.Height = menuItem.Height;

                height += (int)MathF.Ceiling(menuItem.Height);

                _ = PInvoke.GdipMeasureString(graphicsHandle, menuItem.Text, menuItem.Text.Length, font, ref layout, nint.Zero, out var boundingBox, out _, out _);
                if (boundingBox.Width > maxTextWidth) maxTextWidth = boundingBox.Width;
            }
            else height += (int)MathF.Ceiling(menuItems[i].Height);
        }

        _ = PInvoke.GdipDeleteFont(font);
        _ = PInvoke.GdipDeleteGraphics(graphicsHandle);
        _ = PInvoke.DeleteDC(dcHandle);

        width = (int)Math.Ceiling(CheckBoxWidth + TextPadding * 3 + maxTextWidth + SubmenuArrowWidth);
    }
}