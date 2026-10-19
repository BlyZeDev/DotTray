namespace DotTray.Direct;

using DotTray.Direct.Internal;
using DotTray.Direct.Internal.Models;
using DotTray.Direct.Internal.Rendering;
using DotTray.Direct.Primitives;
using System.Runtime.InteropServices;

/// <summary>
/// Manages a popup menu tree
/// </summary>
/// <remarks>
/// The tree owns the shared rendering device, keyboard navigation, the click outside detection and the lifetime of every popup in it
/// </remarks>
public sealed class PopupMenuTree : IDisposable
{
    private readonly PInvoke.LowLevelMouseProc? _pMouseHook;
    private readonly nint _hMouseHook;

    private readonly PInvoke.LowLevelKeyboardProc _pKeyHook;
    private readonly nint _hKeyHook;

    private readonly Func<bool, PopupMenuTree> _recreate;

    private readonly Dictionary<nint, PopupMenu> _popups = [];
    private readonly nint _rootHWnd;

    private nint currentLeafHWnd;
    private bool _disposed;

    internal DirectDevice Device { get; }

    internal PopupMenuStyle Style { get; }

    internal event Action? Disposed;

    private PopupMenuTree(MenuItemCollection items, PopupMenuStyle style, bool destroyOnClickOutside, Func<bool, PopupMenuTree> recreate)
    {
        _recreate = recreate;
        Style = style;

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

        if (destroyOnClickOutside)
        {
            _pMouseHook = LowLevelMouseProcFunc;
            _hMouseHook = PInvoke.SetWindowsHookEx(PInvoke.WH_MOUSE_LL, Marshal.GetFunctionPointerForDelegate(_pMouseHook), nint.Zero, 0);
        }

        _pKeyHook = LowLevelKeyboardProcFunc;
        _hKeyHook = PInvoke.SetWindowsHookEx(PInvoke.WH_KEYBOARD_LL, Marshal.GetFunctionPointerForDelegate(_pKeyHook), nint.Zero, 0);
    }

    /// <summary>
    /// Recreates the current popup window tree
    /// </summary>
    /// <param name="destroyOnClickOutside"><see langword="true"/> if this popup tree should be destroyed when clicked outside, otherwise <see langword="false"/></param>
    /// <returns><see cref="PopupMenuTree"/></returns>
    public PopupMenuTree Regrow(bool destroyOnClickOutside)
    {
        foreach (var hWnd in EnumerateOwnerWindows(currentLeafHWnd, true))
        {
            PInvoke.PostMessage(hWnd, PopupMenu.WM_APP_POPUP_CALCWND, nint.Zero, nint.Zero);
        }

        return _recreate(destroyOnClickOutside);
    }

    /// <summary>
    /// Closes a popup
    /// </summary>
    /// <remarks>
    /// The closed popup is the current leaf popup.<br/>
    /// The owner popup of the closed popup will be the new leaf popup
    /// </remarks>
    public void Close()
    {
        var newLeaf = _popups.TryGetValue(currentLeafHWnd, out var popup) ? popup.OwnerHWnd : nint.Zero;
        PInvoke.PostMessage(currentLeafHWnd, PInvoke.WM_CLOSE, nint.Zero, nint.Zero);
        currentLeafHWnd = newLeaf;
    }

    /// <inheritdoc/>
    public void Dispose() => PInvoke.PostMessage(_rootHWnd, PInvoke.WM_CLOSE, nint.Zero, nint.Zero);

    internal List<Rect> GetOpenWindowRects(nint excludeHWnd)
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

    internal void OpenChild(nint ownerHWnd, MenuItemCollection items, Rect anchorScreenRect, bool selectFirstItem)
    {
        CloseChildrenOf(ownerHWnd);
        CreatePopup(ownerHWnd, items, anchorScreenRect, selectFirstItem);
    }

    internal void CloseChildrenOf(nint hWnd)
    {
        while (currentLeafHWnd != hWnd && currentLeafHWnd != nint.Zero && _popups.ContainsKey(currentLeafHWnd))
        {
            Close();
        }
    }

    internal void NavigateBack()
    {
        if (currentLeafHWnd == _rootHWnd) return;
        Close();
    }

    internal void CloseFromEscape()
    {
        if (currentLeafHWnd == _rootHWnd) Dispose();
        else Close();
    }

    internal void UnregisterWindow(nint hWnd)
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

    private static bool IsNavigationKey(int vkCode) => vkCode is
        PInvoke.VK_UP or PInvoke.VK_DOWN or PInvoke.VK_LEFT or PInvoke.VK_RIGHT or
        PInvoke.VK_RETURN or PInvoke.VK_ESCAPE;

    private bool IsInHierarchy()
    {
        PInvoke.GetCursorPos(out var pos);

        foreach (var hWnd in EnumerateOwnerWindows(currentLeafHWnd, true))
        {
            if (IsHit(hWnd, pos)) return true;
        }

        return false;
    }

    private void DisposeCore()
    {
        if (_disposed) return;
        _disposed = true;

        if (_hMouseHook != nint.Zero) PInvoke.UnhookWindowsHookEx(_hMouseHook);
        PInvoke.UnhookWindowsHookEx(_hKeyHook);

        Disposed?.Invoke();
    }

    /// <summary>
    /// Creates a popup window tree and shows the root popup
    /// </summary>
    /// <param name="handler">The handler that owns the items and style of this tree</param>
    /// <param name="destroyOnClickOutside"><see langword="true"/> if this popup tree should be destroyed when clicked outside, otherwise <see langword="false"/></param>
    /// <returns><see cref="PopupMenuTree"/></returns>
    public static PopupMenuTree Show(DirectPopupMenuHandler handler, bool destroyOnClickOutside)
        => new(handler.MenuItems, handler.Style, destroyOnClickOutside, destroy => Show(handler, destroy));

    private static IEnumerable<nint> EnumerateOwnerWindows(nint leafWindow, bool includeLeafWindow = false)
    {
        if (includeLeafWindow)
        {
            if (leafWindow == nint.Zero) yield break;
            yield return leafWindow;
        }

        while (true)
        {
            leafWindow = PInvoke.GetWindow(leafWindow, PInvoke.GW_OWNER);
            if (leafWindow == nint.Zero) yield break;
            yield return leafWindow;
        }
    }

    private static bool IsHit(nint hWnd, POINT pos) => PInvoke.GetWindowRect(hWnd, out var rect) && PInvoke.PtInRect(ref rect, pos);
}