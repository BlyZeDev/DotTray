namespace DotTray.Internal;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using System;
using System.Runtime.InteropServices;
using System.Threading;

internal sealed class PopupMenu : IDisposable
{
    private const int ItemHeight = 20;
    private const int CheckBoxWidth = 16;
    private const int CheckBoxToTextPadding = 4;
    private const int SubmenuArrowWidth = 12;

    private readonly nint _ownerHWnd;
    private readonly NotifyIcon _ownerIcon;
    private readonly MenuItemCollection _menuItems;

    private readonly nint _hWnd;
    private readonly PInvoke.WndProc _wndProc;

    private PopupMenu? submenuPopup;

    public event Action? Closed;

    private PopupMenu(nint ownerHWnd, NotifyIcon ownerIcon, MenuItemCollection menuItems, POINT mousePos, string popupWindowClassName, nint instanceHandle)
    {
        _ownerHWnd = ownerHWnd;
        _ownerIcon = ownerIcon;
        _menuItems = menuItems;

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

    private void HandlePaint()
    {

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
        width = CheckBoxWidth + CheckBoxToTextPadding + maxTextWidth + SubmenuArrowWidth;
        height = ItemHeight * _menuItems.Count;

        if (x + width > screenWidth) x = screenWidth - width;
        if (y + height > screenHeight) y = screenHeight - height;
    }

    public static PopupMenu Show(nint ownerHWnd, NotifyIcon notifyIcon, MenuItemCollection menuItems, POINT mousePos, string popupWindowClassName, nint instanceHandle)
        => new PopupMenu(ownerHWnd, notifyIcon, menuItems, mousePos, popupWindowClassName, instanceHandle);
}