namespace DotTray.Internal;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[SupportedOSPlatform("Windows")]
internal sealed class PopupMenu : IDisposable
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

    private readonly nint _ownerHWnd;
    private readonly NotifyIcon _ownerIcon;
    private readonly MenuItemCollection _menuItems;
    private readonly string _popupWindowClassName;
    private readonly nint _instanceHandle;

    private readonly nint _hWnd;
    private readonly PInvoke.WndProc _wndProc;
    private readonly PopupDismissHook _popupDismissHook;

    private bool isDisposed;
    private int hoverIndex;
    private PopupMenu? submenuPopup;

    public event Action? Closed;

    static PopupMenu()
    {
        _arrowCursor = PInvoke.LoadCursor(nint.Zero, PInvoke.IDC_ARROW);
        _handCursor = PInvoke.LoadCursor(nint.Zero, PInvoke.IDC_HAND);
    }

    private PopupMenu(nint ownerHWnd, NotifyIcon ownerIcon, MenuItemCollection menuItems, int x, int y, int width, int height, string popupWindowClassName, nint instanceHandle)
    {
        _ownerHWnd = ownerHWnd;
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
            _ownerHWnd,
            nint.Zero,
            _instanceHandle,
            nint.Zero);

        _wndProc = new PInvoke.WndProc(WndProcFunc);
        PInvoke.SetWindowLongPtr(_hWnd, PInvoke.GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_wndProc));

        _popupDismissHook = new PopupDismissHook(_hWnd);
        _popupDismissHook.ClickedOutside += Dispose;

        var cornerRadius = PInvoke.DWMWCP_ROUND;
        _ = PInvoke.DwmSetWindowAttribute(_hWnd, PInvoke.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerRadius, sizeof(int));

        var backdrop = PInvoke.DWMSBT_TRANSIENTWINDOW;
        _ = PInvoke.DwmSetWindowAttribute(_hWnd, PInvoke.DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int));

        PInvoke.ShowWindow(_hWnd, PInvoke.SW_SHOWNOACTIVATE);
        PInvoke.UpdateWindow(_hWnd);
    }

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;

        _popupDismissHook.ClickedOutside -= Dispose;
        _menuItems.Updated -= MenuItemUpdated;

        _popupDismissHook.Dispose();
        submenuPopup?.Dispose();

        PInvoke.PostMessage(_hWnd, PInvoke.WM_CLOSE, 0, 0);
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
            case PInvoke.WM_MOUSEMOVE or PInvoke.WM_LBUTTONDOWN: HandleMouse(msg, lParam); return 0;
            case PInvoke.WM_PAINT: HandlePaint(); return 0;

            case PInvoke.WM_CLOSE: PInvoke.DestroyWindow(hWnd); return 0;
            case PInvoke.WM_DESTROY: Closed?.Invoke(); return 0;
        }

        return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private void HandleMouse(uint msg, nint lParam)
    {
        var mouseX = (short)(lParam.ToInt32() & 0xFFFF);
        var mouseY = (short)((lParam.ToInt32() >> 16) & 0xFFFF);

        var hitIndex = CheckHit(mouseX, mouseY);

        if (msg == PInvoke.WM_MOUSEMOVE && hitIndex != hoverIndex)
        {
            hoverIndex = hitIndex;
            PInvoke.InvalidateRect(_hWnd, nint.Zero, false);

            submenuPopup?.Dispose();
            submenuPopup = null;

            if (hoverIndex != -1 && _menuItems[hoverIndex] is MenuItem menuItem && !menuItem.IsDisabled)
            {
                PInvoke.SetCursor(_handCursor);

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
                        x = topLeft.x - (int)MathF.Ceiling(menuItem.HitBox.Width) - width;

                    if (y + height > PInvoke.GetSystemMetrics(PInvoke.SM_CYSCREEN) - ScreenMargin)
                        y = topLeft.y - (int)MathF.Ceiling(menuItem.HitBox.Height) - height;

                    submenuPopup = new PopupMenu(_hWnd, _ownerIcon, menuItem.SubMenu, x, y, width, height, _popupWindowClassName, _instanceHandle);
                }
            }
            else PInvoke.SetCursor(_arrowCursor);
        }
        else if (msg == PInvoke.WM_LBUTTONDOWN && hitIndex != -1)
        {
            if (_menuItems[hitIndex] is MenuItem menuItem && !menuItem.IsDisabled)
            {
                if (menuItem.IsChecked.HasValue) menuItem.IsChecked = !menuItem.IsChecked;

                menuItem.Clicked?.Invoke(new MenuItemClickedArgs
                {
                    Icon = _ownerIcon,
                    MenuItem = menuItem
                });

                Dispose();
            }
        }
    }

    private unsafe void HandlePaint()
    {
        var paintHandle = PInvoke.BeginPaint(_hWnd, out var paint);
        
        try
        {
            PInvoke.GetClientRect(_hWnd, out var clientRect);

            var memoryDC = PInvoke.CreateCompatibleDC(paintHandle);
            var bitmapHandle = PInvoke.CreateCompatibleBitmap(paintHandle, clientRect.Right - clientRect.Left, clientRect.Bottom - clientRect.Top);
            var oldBitmapHandle = PInvoke.SelectObject(memoryDC, bitmapHandle);

            _ = PInvoke.GdipCreateFromHDC(memoryDC, out var graphicsHandle);
            _ = PInvoke.GdipSetSmoothingMode(graphicsHandle, PInvoke.SmoothingModeAntiAlias8x8);

            DrawMenuBackground(graphicsHandle, clientRect, _ownerIcon.PopupMenuColor.ToGdiPlus());

            _ = PInvoke.GdipGetGenericFontFamilySansSerif(out var fontFamily);
            _ = PInvoke.GdipCreateFont(fontFamily, FontSize, 0, PInvoke.UnitPixel, out var font);

            POINTF* checkBoxPoints = stackalloc POINTF[CheckBoxPoints];
            POINTF* submenuArrowPoints = stackalloc POINTF[ArrowPoints];
            var itemTop = (float)clientRect.Top;
            for (int i = 0; i < _menuItems.Count; i++)
            {
                var itemRect = new RECTF
                {
                    X = clientRect.Left,
                    Y = itemTop,
                    Width = clientRect.Right - clientRect.Left,
                    Height = _menuItems[i].Height
                };
                
                if (_menuItems[i] is MenuItem menuItem)
                {
                    menuItem.HitBox = itemRect;

                    var backgroundColor = (menuItem.IsDisabled ? menuItem.BackgroundDisabledColor : (i == hoverIndex ? menuItem.BackgroundHoverColor : menuItem.BackgroundColor)).ToGdiPlus();
                    var textColor = (menuItem.IsDisabled ? menuItem.TextDisabledColor : (i == hoverIndex ? menuItem.TextHoverColor : menuItem.TextColor)).ToGdiPlus();

                    DrawMenuItem(graphicsHandle, menuItem, font, backgroundColor, textColor, checkBoxPoints, submenuArrowPoints);
                }
                else if (_menuItems[i] is SeparatorItem separatorItem) DrawSeparatorItem(graphicsHandle, separatorItem, itemRect);

                itemTop += itemRect.Height;
            }

            _ = PInvoke.GdipDeleteFont(font);
            _ = PInvoke.GdipDeleteFontFamily(fontFamily);
            _ = PInvoke.GdipDeleteGraphics(graphicsHandle);

            PInvoke.BitBlt(paintHandle, 0, 0, clientRect.Right - clientRect.Left, clientRect.Bottom - clientRect.Top, memoryDC, 0, 0, PInvoke.SRCCOPY);

            _ = PInvoke.SelectObject(memoryDC, oldBitmapHandle);
            _ = PInvoke.DeleteObject(bitmapHandle);
            _ = PInvoke.DeleteDC(memoryDC);
        }
        finally
        {
            PInvoke.EndPaint(_hWnd, ref paint);
        }
    }

    private int CheckHit(int mouseX, int mouseY)
    {
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
        return new PopupMenu(ownerHWnd, notifyIcon, notifyIcon.MenuItems, x, y, width, height, popupWindowClassName, instanceHandle);
    }

    private static void DrawMenuBackground(nint graphicsHandle, RECT clientRect, uint color)
    {
        _ = PInvoke.GdipCreateSolidFill(color, out var bgBrush);

        _ = PInvoke.GdipFillRectangle(
            graphicsHandle,
            bgBrush,
            clientRect.Left,
            clientRect.Top,
            clientRect.Right - clientRect.Left,
            clientRect.Bottom - clientRect.Top);

        _ = PInvoke.GdipDeleteBrush(bgBrush);
    }
    
    private static unsafe void DrawMenuItem(nint graphicsHandle, MenuItem menuItem, nint font, uint backgroundColor, uint textColor, POINTF* checkBoxPoints, POINTF* submenuArrowPoints)
    {
        var itemRect = menuItem.HitBox;

        _ = PInvoke.GdipCreateSolidFill(backgroundColor, out var backgroundBrush);
        _ = PInvoke.GdipFillRectangle(graphicsHandle, backgroundBrush, itemRect.X, itemRect.Y, itemRect.Width, itemRect.Height);
        _ = PInvoke.GdipDeleteBrush(backgroundBrush);

        var centerY = itemRect.Y + itemRect.Height / 2f - TextPadding - 1.65f;

        if (menuItem.IsChecked.GetValueOrDefault()) DrawCheckBox(graphicsHandle, textColor, itemRect, centerY, checkBoxPoints);

        var textLeft = itemRect.X + CheckBoxWidth + TextPadding;
        var textRight = itemRect.X + itemRect.Width - SubmenuArrowWidth - TextPadding;
        var textRect = new RECTF
        {
            X = textLeft,
            Y = itemRect.Y,
            Width = textRight - textLeft,
            Height = itemRect.Height
        };
        DrawText(graphicsHandle, menuItem.Text, font, textColor, textRect, centerY);

        if (menuItem.HasSubMenu) DrawSubmenuArrow(graphicsHandle, textColor, itemRect, centerY + TextPadding + 0.5f, submenuArrowPoints);
    }

    private static void DrawSeparatorItem(nint graphicsHandle, SeparatorItem separatorItem, RECTF itemRect)
    {
        _ = PInvoke.GdipCreateSolidFill(separatorItem.BackgroundColor.ToGdiPlus(), out var backgroundBrush);
        _ = PInvoke.GdipFillRectangle(graphicsHandle, backgroundBrush, itemRect.X, itemRect.Y, itemRect.Width, itemRect.Height);
        _ = PInvoke.GdipDeleteBrush(backgroundBrush);

        var y = itemRect.Y + itemRect.Height / 2;
        _ = PInvoke.GdipCreatePen1(separatorItem.LineColor.ToGdiPlus(), separatorItem.LineThickness, PInvoke.UnitPixel, out var pen);
        _ = PInvoke.GdipDrawLine(graphicsHandle, pen, itemRect.X + SeparatorPadding, y, itemRect.X + itemRect.Width - SeparatorPadding - 1f, y);
        _ = PInvoke.GdipDeletePen(pen);
    }

    private static unsafe void DrawCheckBox(nint graphicsHandle, uint color, RECTF itemRect, float centerY, POINTF* points)
    {
        var checkX = itemRect.X + TextPadding / 1.35f;
        var checkY = centerY;

        points[0] = new POINTF { X = checkX + CheckBoxWidth * 0.10f, Y = checkY + CheckBoxWidth * 0.55f };
        points[1] = new POINTF { X = checkX + CheckBoxWidth * 0.40f, Y = checkY + CheckBoxWidth * 0.85f };
        points[2] = new POINTF { X = checkX + CheckBoxWidth * 0.90f, Y = checkY + CheckBoxWidth * 0.15f };

        _ = PInvoke.GdipCreatePen1(color, 3f, PInvoke.UnitPixel, out var pen);
        _ = PInvoke.GdipDrawLines(graphicsHandle, pen, points, CheckBoxPoints);
        _ = PInvoke.GdipDeletePen(pen);
    }

    private static void DrawText(nint graphicsHandle, string text, nint font, uint color, RECTF textRect, float centerY)
    {
        _ = PInvoke.GdipCreateSolidFill(color, out var textBrush);
        _ = PInvoke.GdipCreateStringFormat(0, 0, out var format);
        _ = PInvoke.GdipSetStringFormatFlags(format, PInvoke.StringFormatFlagsNoWrap);

        textRect.Y = centerY;

        _ = PInvoke.GdipDrawString(graphicsHandle, text, text.Length, font, ref textRect, format, textBrush);
        _ = PInvoke.GdipDeleteBrush(textBrush);
        _ = PInvoke.GdipDeleteStringFormat(format);
    }

    private static unsafe void DrawSubmenuArrow(nint graphicsHandle, uint color, RECTF itemRect, float centerY, POINTF* points)
    {
        var arrowX = itemRect.X + itemRect.Width - TextPadding - SubmenuArrowWidth;

        points[0] = new POINTF { X = arrowX, Y = centerY - 5f };
        points[1] = new POINTF { X = arrowX + SubmenuArrowWidth, Y = centerY };
        points[2] = new POINTF { X = arrowX, Y = centerY + 5f };

        _ = PInvoke.GdipCreateSolidFill(color, out var brush);
        _ = PInvoke.GdipFillPolygon(graphicsHandle, brush, points, ArrowPoints, 0);
        _ = PInvoke.GdipDeleteBrush(brush);
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