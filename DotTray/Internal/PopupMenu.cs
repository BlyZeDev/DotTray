namespace DotTray.Internal;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[SupportedOSPlatform("Windows")]
internal sealed class PopupMenu : IDisposable
{
    private const int ItemHeight = 22;
    private const int CheckBoxWidth = 16;
    private const int TextPadding = 4;
    private const int SubmenuArrowWidth = 12;
    private const int SeparatorPadding = (CheckBoxWidth + TextPadding + SubmenuArrowWidth) / 2;

    private readonly nint _ownerHWnd;
    private readonly NotifyIcon _ownerIcon;
    private readonly MenuItemCollection _menuItems;

    private readonly nint _hWnd;
    private readonly PInvoke.WndProc _wndProc;

    private int hoverIndex;
    private PopupMenu? submenuPopup;

    public event Action? Closed;

    private PopupMenu(nint ownerHWnd, NotifyIcon ownerIcon, POINT mousePos, string popupWindowClassName, nint instanceHandle)
    {
        _ownerHWnd = ownerHWnd;
        _ownerIcon = ownerIcon;
        _menuItems = ownerIcon.MenuItems;

        hoverIndex = -1;

        CalcWindowPos(mousePos, out var x, out var y, out var width, out var height);

        _hWnd = PInvoke.CreateWindowEx(
            PInvoke.WS_EX_NOACTIVATE | PInvoke.WS_EX_TOOLWINDOW | PInvoke.WS_EX_TOPMOST,
            popupWindowClassName, "",
            PInvoke.WS_CLIPCHILDREN | PInvoke.WS_CLIPSIBLINGS | PInvoke.WS_POPUP | PInvoke.WS_VISIBLE,
            x, y, width, height,
            _ownerHWnd,
            nint.Zero,
            instanceHandle,
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

    public void Dispose() => PInvoke.PostMessage(_hWnd, PInvoke.WM_CLOSE, 0, 0);

    private nint WndProcFunc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case PInvoke.WM_PAINT: HandlePaint(); return 0;
            case PInvoke.WM_CLOSE: PInvoke.DestroyWindow(hWnd); return 0;
            case PInvoke.WM_DESTROY: Closed?.Invoke(); return 0;
        }

        return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private unsafe void HandlePaint()
    {
        var paintHandle = PInvoke.BeginPaint(_hWnd, out var paint);
        
        try
        {
            PInvoke.GetClientRect(_hWnd, out var clientRect);

            _ = PInvoke.GdipCreateFromHDC(paintHandle, out var graphicsHandle);
            _ = PInvoke.GdipSetSmoothingMode(graphicsHandle, PInvoke.SmoothingModeAntiAlias8x8);

            _ = PInvoke.GdipCreateSolidFill(_ownerIcon.PopupMenuColor.ToGdiPlus(), out var bgBrush);

            _ = PInvoke.GdipFillRectangle(
                graphicsHandle,
                bgBrush,
                clientRect.Left,
                clientRect.Top,
                clientRect.Right - clientRect.Left,
                clientRect.Bottom - clientRect.Top);

            _ = PInvoke.GdipDeleteBrush(bgBrush);

            const int Points = 3;
            POINTF* points = stackalloc POINTF[Points];
            for (int i = 0; i < _menuItems.Count; i++)
            {
                var itemTop = clientRect.Top + i * ItemHeight;
                var itemRect = new RECT
                {
                    Left = clientRect.Left,
                    Top = itemTop,
                    Right = clientRect.Right,
                    Bottom = itemTop + ItemHeight,
                };

                if (_menuItems[i] is MenuItem menuItem)
                {
                    var backgroundColor = (menuItem.IsDisabled
                        ? menuItem.BackgroundDisabledColor : (i == hoverIndex ? menuItem.BackgroundHoverColor : menuItem.BackgroundColor)).ToGdiPlus();

                    _ = PInvoke.GdipCreateSolidFill(backgroundColor, out var hoverBrush);

                    _ = PInvoke.GdipFillRectangle(
                        graphicsHandle,
                        hoverBrush,
                        itemRect.Left,
                        itemRect.Top,
                        itemRect.Right - itemRect.Left,
                        ItemHeight);

                    var textColor = (menuItem.IsDisabled
                        ? menuItem.TextDisabledColor : (i == hoverIndex ? menuItem.TextHoverColor : menuItem.TextColor)).ToGdiPlus();

                    var checkLeft = itemRect.Left + TextPadding;
                    var checkSize = CheckBoxWidth;
                    var checkTop = itemRect.Top + (ItemHeight - checkSize) / 2;

                    var textLeft = checkLeft + checkSize + TextPadding;
                    var textRight = itemRect.Right - SubmenuArrowWidth - TextPadding;

                    var arrowLeft = itemRect.Right - SubmenuArrowWidth + TextPadding;
                    var arrowTop = itemRect.Top + (ItemHeight - SubmenuArrowWidth) / 2;

                    if (menuItem.IsChecked.GetValueOrDefault())
                    {
                        _ = PInvoke.GdipCreatePen1(textColor, 2f, PInvoke.UnitPixel, out var pen);

                        points[0] = new POINTF { X = itemRect.Left + 3.5f, Y = itemRect.Top + 8f };
                        points[1] = new POINTF { X = itemRect.Left + 7f, Y = itemRect.Top + 12.5f };
                        points[2] = new POINTF { X = itemRect.Left + 12.5f, Y = itemRect.Top + 4.5f };

                        _ = PInvoke.GdipDrawLines(graphicsHandle, pen, points, Points);
                        _ = PInvoke.GdipDeletePen(pen);
                    }

                    var textRect = new RECT
                    {
                        Left = textLeft,
                        Top = itemRect.Top,
                        Right = textRight,
                        Bottom = itemRect.Bottom
                    };
                    _ = PInvoke.GdipCreateSolidFill(textColor, out var textBrush);
                    _ = PInvoke.GdipDrawString(graphicsHandle, menuItem.Text, menuItem.Text.Length, nint.Zero, ref textRect, nint.Zero, textBrush);
                    _ = PInvoke.GdipDeleteBrush(textBrush);

                    _ = PInvoke.GdipDeleteBrush(hoverBrush);
                }
                else if (_menuItems[i] is SeparatorItem separatorItem)
                {
                    var y = itemRect.Top + ItemHeight / 2;
                    _ = PInvoke.GdipCreatePen1(separatorItem.LineColor.ToGdiPlus(), separatorItem.LineThickness, PInvoke.UnitPixel, out var pen);
                    _ = PInvoke.GdipDrawLine(graphicsHandle, pen, itemRect.Left + SeparatorPadding, y, itemRect.Right - SeparatorPadding - 1f, y);
                    _ = PInvoke.GdipDeletePen(pen);
                }
            }

            _ = PInvoke.GdipDeleteGraphics(graphicsHandle);
        }
        finally
        {
            PInvoke.EndPaint(_hWnd, ref paint);
        }
    }

    private void CalcWindowPos(POINT mousePos, out int x, out int y, out int width, out int height)
    {
        var screenWidth = PInvoke.GetSystemMetrics(PInvoke.SM_CXSCREEN);
        var screenHeight = PInvoke.GetSystemMetrics(PInvoke.SM_CYSCREEN);

        var dcHandle = PInvoke.CreateCompatibleDC(nint.Zero);
        var maxTextWidth = 0;
        try
        {
            var textRect = new RECT();
            foreach (var item in _menuItems)
            {
                if (item is not MenuItem menuItem) continue;

                _ = PInvoke.DrawText(dcHandle, menuItem.Text, -1, ref textRect, PInvoke.DT_CALCRECT);
                if (textRect.Right - textRect.Left > maxTextWidth) maxTextWidth = textRect.Right - textRect.Left;
            }
        }
        finally
        {
            PInvoke.DeleteDC(dcHandle);
        }

        x = mousePos.x;
        y = mousePos.y;
        width = CheckBoxWidth + TextPadding + maxTextWidth + SubmenuArrowWidth;
        height = ItemHeight * _menuItems.Count;

        if (x + width > screenWidth) x = screenWidth - width;
        if (y + height > screenHeight) y = screenHeight - height;
    }

    public static PopupMenu Show(nint ownerHWnd, NotifyIcon notifyIcon, POINT mousePos, string popupWindowClassName, nint instanceHandle)
        => new PopupMenu(ownerHWnd, notifyIcon, mousePos, popupWindowClassName, instanceHandle);
}