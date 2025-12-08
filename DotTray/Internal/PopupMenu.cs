namespace DotTray.Internal;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[SupportedOSPlatform("Windows")]
internal sealed partial class PopupMenu : IDisposable
{
    private const int ScreenMargin = 10;

    private const float FontSize = 16f;

    private const int CheckBoxPoints = 3;
    private const int CheckBoxWidth = 16;
    private const int TextPadding = 8;
    private const int ArrowPoints = 3;
    private const int SubmenuArrowWidth = 8;
    private const int SeparatorPadding = (CheckBoxWidth + TextPadding + SubmenuArrowWidth) / 4;

    private static readonly nint _arrowCursor;
    private static readonly nint _handCursor;

    private readonly nint _parentHWnd;
    private readonly NotifyIcon _ownerIcon;
    private readonly MenuItemCollection _menuItems;
    private readonly string _popupWindowClassName;
    private readonly nint _instanceHandle;

    private readonly nint _hWnd;
    private readonly PInvoke.WndProc _wndProc;

    private bool isClosed;
    private bool isMouseTracking;
    private int hoverIndex;

    public event Action? Closed;

    static PopupMenu()
    {
        _arrowCursor = PInvoke.LoadCursor(nint.Zero, PInvoke.IDC_ARROW);
        _handCursor = PInvoke.LoadCursor(nint.Zero, PInvoke.IDC_HAND);
    }

    private PopupMenu(nint parentHWnd, NotifyIcon ownerIcon, MenuItemCollection menuItems, int x, int y, int width, int height, string popupWindowClassName, nint instanceHandle)
    {
        _parentHWnd = parentHWnd;
        _ownerIcon = ownerIcon;
        _menuItems = menuItems;
        _popupWindowClassName = popupWindowClassName;
        _instanceHandle = instanceHandle;

        _menuItems.Updated += MenuItemUpdated;

        hoverIndex = -1;

        _hWnd = PInvoke.CreateWindowEx(
            PInvoke.WS_EX_NOACTIVATE | PInvoke.WS_EX_TOOLWINDOW | PInvoke.WS_EX_TOPMOST,
            _popupWindowClassName, "",
            PInvoke.WS_CLIPCHILDREN | PInvoke.WS_CLIPSIBLINGS | PInvoke.WS_POPUP | PInvoke.WS_VISIBLE,
            x, y, width, height,
            _parentHWnd,
            nint.Zero,
            _instanceHandle,
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

    public void Close()
    {
        if (isClosed) return;
        isClosed = true;

        childPopup?.Close();

        PInvoke.PostMessage(_hWnd, PInvoke.WM_CLOSE, 0, 0);
    }

    public void CloseAll()
    {

    }

    public void Dispose() => _menuItems.Updated -= MenuItemUpdated;

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
            case PInvoke.WM_MOUSEMOVE: HandleMouseMove(CheckHit(lParam)); return 0;
            case PInvoke.WM_MOUSELEAVE: HandleMouseLeave(); return 0;
            case PInvoke.WM_LBUTTONUP or PInvoke.WM_RBUTTONUP or PInvoke.WM_MBUTTONUP: HandleMouseUp(CheckHit(lParam), msg); return 0;
            case PInvoke.WM_PAINT: HandlePaint(); return 0;

            case PInvoke.WM_CLOSE: PInvoke.DestroyWindow(hWnd); return 0;
            case PInvoke.WM_DESTROY: Closed?.Invoke(); return 0;
        }

        return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
    }
    
    private void HandleMouseMove(int hitIndex)
    {
        if (!isMouseTracking)
        {
            var tme = new TRACKMOUSEEVENT
            {
                cbSize = (uint)Marshal.SizeOf<TRACKMOUSEEVENT>(),
                dwFlags = PInvoke.TME_LEAVE,
                hwndTrack = _hWnd,
                dwHoverTime = 0
            };

            isMouseTracking = PInvoke.TrackMouseEvent(ref tme);
        }

        if (hitIndex == hoverIndex) return;

        hoverIndex = hitIndex;
        PInvoke.InvalidateRect(_hWnd, nint.Zero, false);

        if (hoverIndex != -1 && _menuItems[hoverIndex] is MenuItem menuItem && !menuItem.IsDisabled)
        {
            PInvoke.SetCursor(_handCursor);
            childPopup?.Close();
            childPopup = null;

            if (menuItem.HasSubMenu)
            {
                CalcWindowSize(menuItem.SubMenu, out var width, out var height);

                var topLeft = new POINT
                {
                    x = (int)MathF.Ceiling(menuItem.HitBox.X + menuItem.HitBox.Width),
                    y = (int)MathF.Ceiling(menuItem.HitBox.Y)
                };
                PInvoke.ClientToScreen(_hWnd, ref topLeft);

                var x = topLeft.x;
                var y = topLeft.y;
                
                if (x + width > PInvoke.GetSystemMetrics(PInvoke.SM_CXSCREEN) - ScreenMargin)
                    x = x - (int)MathF.Ceiling(menuItem.HitBox.Width) - width;

                if (y + height > PInvoke.GetSystemMetrics(PInvoke.SM_CYSCREEN) - ScreenMargin)
                    y = y - (int)MathF.Ceiling(menuItem.HitBox.Height) - height;
                
                childPopup = new PopupMenu(_rootPopup, _hWnd, _ownerIcon, menuItem.SubMenu, x, y, width, height, _popupWindowClassName, _instanceHandle);
            }
        }
        else PInvoke.SetCursor(_arrowCursor);
    }

    private void HandleMouseLeave()
    {
        isMouseTracking = false;
        hoverIndex = -1;

        PInvoke.InvalidateRect(_hWnd, nint.Zero, false);
    }

    private void HandleMouseUp(int hitIndex, uint msg)
    {
        if (hitIndex == -1) return;

        if (_menuItems[hitIndex] is MenuItem menuItem && !menuItem.IsDisabled)
        {
            if (menuItem.IsChecked.HasValue) menuItem.IsChecked = !menuItem.IsChecked;

            menuItem.Clicked?.Invoke(new MenuItemClickedArgs
            {
                Icon = _ownerIcon,
                MenuItem = menuItem,
                MouseButton = msg switch
                {
                    PInvoke.WM_LBUTTONUP => MouseButton.Left,
                    PInvoke.WM_RBUTTONUP => MouseButton.Right,
                    PInvoke.WM_MBUTTONUP => MouseButton.Middle,
                    _ => MouseButton.None
                }
            });

            CloseAll();
        }
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

    public static PopupMenu Show(nint ownerHWnd, NotifyIcon notifyIcon, POINT mousePos, string popupWindowClassName, nint instanceHandle)
    {
        CalcWindowSize(notifyIcon.MenuItems, out var width, out var height);
        CalcWindowPos(mousePos, width, height, out var x, out var y);

        return new PopupMenu(null, ownerHWnd, notifyIcon, notifyIcon.MenuItems, x, y, width, height, popupWindowClassName, instanceHandle);
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