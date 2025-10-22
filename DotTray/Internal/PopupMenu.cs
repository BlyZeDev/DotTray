namespace DotTray.Internal;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using System;
using System.Runtime.InteropServices;

internal sealed class PopupMenu : IDisposable
{
    private const int ItemHeight = 20;
    private const int CheckBoxWidth = 16;
    private const int TextPadding = 4;
    private const int SubmenuArrowWidth = 12;

    private readonly nint _ownerHWnd;
    private readonly NotifyIcon _ownerIcon;
    private readonly MenuItemCollection _menuItems;

    private readonly nint _hWnd;
    private readonly PInvoke.WndProc _wndProc;

    private int hoverIndex;
    private PopupMenu? submenuPopup;

    public event Action? Closed;

    private PopupMenu(nint ownerHWnd, NotifyIcon ownerIcon, MenuItemCollection menuItems, POINT mousePos, string popupWindowClassName, nint instanceHandle)
    {
        _ownerHWnd = ownerHWnd;
        _ownerIcon = ownerIcon;
        _menuItems = menuItems;

        hoverIndex = -1;

        CalcWindowPos(mousePos, out var x, out var y, out var width, out var height);

        _hWnd = PInvoke.CreateWindowEx(
            PInvoke.WS_EX_NOACTIVATE | PInvoke.WS_EX_TOOLWINDOW | PInvoke.WS_EX_TOPMOST,
            popupWindowClassName, "",
            PInvoke.WS_BORDER | PInvoke.WS_CLIPCHILDREN | PInvoke.WS_CLIPSIBLINGS | PInvoke.WS_POPUP | PInvoke.WS_VISIBLE,
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
            var dcHandle = PInvoke.CreateCompatibleDC(paintHandle);

            PInvoke.GetClientRect(_hWnd, out var clientRect);

            var bmpHandle = PInvoke.CreateCompatibleBitmap(paintHandle, clientRect.Right, clientRect.Bottom);
            var oldBmpHandle = PInvoke.SelectObject(dcHandle, bmpHandle);

            var bgBrush = PInvoke.CreateSolidBrush(new Rgb(80, 80, 80));
            PInvoke.FillRect(dcHandle, ref clientRect, bgBrush);
            PInvoke.DeleteObject(bgBrush);

            var rect = new RECT();
            for (int i = 0; i < _menuItems.Count; i++)
            {
                rect.Left = 0;
                rect.Top = i * ItemHeight;
                rect.Right = clientRect.Right;
                rect.Bottom = (i + 1) * ItemHeight;

                if (i == hoverIndex)
                {
                    var hoverBrush = PInvoke.CreateSolidBrush(new Rgb(100, 100, 100));
                    PInvoke.FillRect(dcHandle, ref rect, hoverBrush);
                    PInvoke.DeleteObject(hoverBrush);
                }

                if (_menuItems[i] is MenuItem menuItem)
                {
                    rect = new RECT
                    {
                        Left = TextPadding,
                        Top = rect.Top + (ItemHeight - CheckBoxWidth) / 2,
                        Right = SubmenuArrowWidth,
                        Bottom = rect.Top + (ItemHeight + CheckBoxWidth) / 2
                    };

                    if (menuItem.IsChecked.GetValueOrDefault())
                    {
                        var penHandle = PInvoke.CreatePen(PInvoke.PS_SOLID, 2, new Rgb(255, 255, 255));
                        var oldPenHandle = PInvoke.SelectObject(dcHandle, penHandle);

                        PInvoke.MoveToEx(dcHandle, rect.Left + TextPadding, rect.Top + CheckBoxWidth / 2, nint.Zero);
                        PInvoke.LineTo(dcHandle, rect.Left + SubmenuArrowWidth / 2, rect.Top + SubmenuArrowWidth);
                        PInvoke.LineTo(dcHandle, rect.Left + CheckBoxWidth, rect.Top + TextPadding);

                        PInvoke.SelectObject(dcHandle, oldPenHandle);
                        PInvoke.DeleteObject(penHandle);
                    }

                    var textColor = menuItem.IsDisabled ? new Rgb(0, 0, 0) : new Rgb(150, 150, 150);
                    _ = PInvoke.SetTextColor(dcHandle, textColor);
                    _ = PInvoke.SetBkMode(dcHandle, PInvoke.TRANSPARENT);

                    PInvoke.TextOut(dcHandle, rect.Right + SubmenuArrowWidth / 2, rect.Top + (ItemHeight - CheckBoxWidth) / 2, menuItem.Text, menuItem.Text.Length);

                    if (menuItem.SubMenu.Count > 0)
                    {
                        var arrowX = rect.Right - SubmenuArrowWidth;
                        var arrowY = rect.Top + ItemHeight / 2;

                        Span<POINT> points =
                        [
                            new POINT { x = arrowX, y = arrowY - TextPadding },
                            new POINT { x = arrowX + SubmenuArrowWidth / 2, y = arrowY },
                            new POINT { x = arrowX, y = arrowY + TextPadding }
                        ];

                        var arrowBrush = PInvoke.CreateSolidBrush(textColor);
                        var oldBrush = PInvoke.SelectObject(dcHandle, arrowBrush);

                        fixed (POINT* pointsPtr = points)
                        {
                            PInvoke.Polygon(dcHandle, pointsPtr, points.Length);
                        }

                        PInvoke.SelectObject(dcHandle, oldBrush);
                        PInvoke.DeleteObject(arrowBrush);
                    }
                }
                else
                {
                    var separatorY = (rect.Top + rect.Bottom) / 2;
                    var penHandle = PInvoke.CreatePen(PInvoke.PS_SOLID, 1, new Rgb(255, 0, 0));
                    var oldPenHandle = PInvoke.SelectObject(dcHandle, penHandle);

                    PInvoke.MoveToEx(dcHandle, TextPadding, separatorY, nint.Zero);
                    PInvoke.LineTo(dcHandle, clientRect.Right - TextPadding, separatorY);
                    PInvoke.SelectObject(dcHandle, oldPenHandle);
                    PInvoke.DeleteObject(penHandle);
                }
            }

            PInvoke.BitBlt(paintHandle, 0, 0, clientRect.Right, clientRect.Bottom, dcHandle, 0, 0, PInvoke.SRCCOPY);

            PInvoke.SelectObject(dcHandle, oldBmpHandle);
            PInvoke.DeleteObject(bmpHandle);
            PInvoke.DeleteDC(dcHandle);
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

    public static PopupMenu Show(nint ownerHWnd, NotifyIcon notifyIcon, MenuItemCollection menuItems, POINT mousePos, string popupWindowClassName, nint instanceHandle)
        => new PopupMenu(ownerHWnd, notifyIcon, menuItems, mousePos, popupWindowClassName, instanceHandle);
}