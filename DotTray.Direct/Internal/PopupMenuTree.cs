namespace DotTray.Direct.Internal;

using DotTray.Direct.Internal.Models;
using DotTray.Direct.Internal.Rendering;
using DotTray.Direct.Primitives;
using System.Runtime.InteropServices;

internal sealed class PopupMenuTree : IDisposable
{
    private readonly PInvoke.LowLevelMouseProc _pMouseHook;
    private readonly nint _hMouseHook;

    private readonly PInvoke.LowLevelKeyboardProc _pKeyHook;
    private readonly nint _hKeyHook;

    private readonly Dictionary<nint, PopupMenu> _popups = [];
    private readonly nint _rootHWnd;

    private nint currentLeafHWnd;
    private bool disposed;

    public DirectPopupMenuHandler Owner { get; }
    public DirectDevice Device { get; }

    public event Action? Disposed;

    public PopupMenuTree(DirectPopupMenuHandler owner, MenuItemCollection items)
    {
        Owner = owner;

        PInvoke.SetThreadDpiAwarenessContext(PInvoke.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);

        Device = new DirectDevice();

        try
        {
            _rootHWnd = CreatePopup(nint.Zero, items, null, false).HWnd;
        }
        catch
        {
            Device.Dispose();
            throw;
        }

        _pMouseHook = LowLevelMouseProcFunc;
        _hMouseHook = PInvoke.SetWindowsHookEx(PInvoke.WH_MOUSE_LL, Marshal.GetFunctionPointerForDelegate(_pMouseHook), nint.Zero, 0);

        _pKeyHook = LowLevelKeyboardProcFunc;
        _hKeyHook = PInvoke.SetWindowsHookEx(PInvoke.WH_KEYBOARD_LL, Marshal.GetFunctionPointerForDelegate(_pKeyHook), nint.Zero, 0);
    }

    public void Close()
    {
        var newLeaf = _popups.TryGetValue(currentLeafHWnd, out var popup) ? popup.OwnerHWnd : nint.Zero;

        PInvoke.PostMessage(currentLeafHWnd, PInvoke.WM_CLOSE, nint.Zero, nint.Zero);
        currentLeafHWnd = newLeaf;
    }

    public void Dispose() => PInvoke.PostMessage(_rootHWnd, PInvoke.WM_CLOSE, nint.Zero, nint.Zero);

    public IReadOnlyCollection<Rect> GetOpenWindowRects(nint excludeHWnd)
    {
        var rects = new List<Rect>(_popups.Count);

        foreach (var hWnd in _popups.Keys)
        {
            if (hWnd == excludeHWnd) continue;
            if (!PInvoke.GetWindowRect(hWnd, out var rect)) continue;

            rects.Add(new Rect(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top));
        }

        return rects;
    }

    public void OpenChild(nint ownerHWnd, MenuItemCollection items, Rect anchorScreenRect, bool selectFirstItem)
    {
        CloseChildrenOf(ownerHWnd);
        CreatePopup(ownerHWnd, items, anchorScreenRect, selectFirstItem);
    }

    public void CloseChildrenOf(nint hWnd)
    {
        while (currentLeafHWnd != hWnd && currentLeafHWnd != nint.Zero && _popups.ContainsKey(currentLeafHWnd))
        {
            Close();
        }
    }

    public void NavigateBack()
    {
        if (currentLeafHWnd == _rootHWnd) return;
        Close();
    }

    public void CloseFromEscape()
    {
        if (currentLeafHWnd == _rootHWnd) Dispose();
        else Close();
    }

    public void UnregisterWindow(nint hWnd)
    {
        _popups.Remove(hWnd);

        if (hWnd == _rootHWnd) DisposeCore();
        if (_popups.Count == 0) Device.Dispose();
    }

    private PopupMenu CreatePopup(nint ownerHWnd, MenuItemCollection items, Rect? anchorScreenRect, bool selectFirstItem)
    {
        var popup = new PopupMenu(this, ownerHWnd, items, anchorScreenRect, selectFirstItem);

        _popups[popup.HWnd] = popup;
        currentLeafHWnd = popup.HWnd;

        return popup;
    }

    private nint LowLevelMouseProcFunc(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0 && wParam is PInvoke.WM_LBUTTONDOWN or PInvoke.WM_RBUTTONDOWN or PInvoke.WM_MBUTTONDOWN)
        {
            if (!IsInHierarchy()) Dispose();
        }

        return PInvoke.CallNextHookEx(_hMouseHook, nCode, wParam, lParam);
    }

    private nint LowLevelKeyboardProcFunc(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0 && wParam is PInvoke.WM_KEYDOWN or PInvoke.WM_SYSKEYDOWN)
        {
            var vkCode = Marshal.ReadInt32(lParam);

            if (IsNavigationKey(vkCode))
            {
                PInvoke.PostMessage(currentLeafHWnd, PopupMenu.WM_APP_POPUP_KEYDOWN, vkCode, nint.Zero);
                return 1;
            }
        }

        return PInvoke.CallNextHookEx(_hKeyHook, nCode, wParam, lParam);
    }

    private bool IsInHierarchy()
    {
        PInvoke.GetCursorPos(out var pos);

        foreach (var hWnd in _popups.Keys)
        {
            if (IsHit(hWnd, pos)) return true;
        }

        return false;
    }

    private void DisposeCore()
    {
        if (disposed) return;
        disposed = true;
        
        PInvoke.UnhookWindowsHookEx(_hMouseHook);
        PInvoke.UnhookWindowsHookEx(_hKeyHook);

        Disposed?.Invoke();
    }

    private static bool IsNavigationKey(int vkCode) => vkCode is
        PInvoke.VK_UP or PInvoke.VK_DOWN or PInvoke.VK_LEFT or PInvoke.VK_RIGHT or
        PInvoke.VK_RETURN or PInvoke.VK_ESCAPE;

    private static bool IsHit(nint hWnd, POINT pos) => PInvoke.GetWindowRect(hWnd, out var rect) && PInvoke.PtInRect(ref rect, pos);
}