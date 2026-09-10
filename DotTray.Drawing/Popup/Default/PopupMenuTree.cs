namespace DotTray.Popup.Default;

using DotTray;
using DotTray.Internal;
using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using DotTray.Primitives;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/// <summary>
/// Manages a popup menu tree
/// </summary>
public sealed class PopupMenuTree : IDisposable
{
    private readonly PInvoke.LowLevelMouseProc? _pMouseHook;
    private readonly nint _hMouseHook;

    private readonly PInvoke.LowLevelKeyboardProc _pKeyHook;
    private readonly nint _hKeyHook;

    private readonly nint _rootHWnd;
    private readonly Dictionary<nint, nint> _ownerByHWnd = [];

    private nint currentLeafHWnd;
    private bool _disposed;

    internal NotifyIcon<DefaultPopupMenuHandler> Owner { get; }

    internal event Action? Disposed;

    private PopupMenuTree(NotifyIcon<DefaultPopupMenuHandler> owner, bool destroyOnClickOutside)
    {
        Owner = owner;

        if (destroyOnClickOutside)
        {
            _pMouseHook = LowLevelMouseProcFunc;
            _hMouseHook = PInvoke.SetWindowsHookEx(PInvoke.WH_MOUSE_LL, Marshal.GetFunctionPointerForDelegate(_pMouseHook), nint.Zero, 0);
        }

        _pKeyHook = LowLevelKeyboardProcFunc;
        _hKeyHook = PInvoke.SetWindowsHookEx(PInvoke.WH_KEYBOARD_LL, Marshal.GetFunctionPointerForDelegate(_pKeyHook), nint.Zero, 0);

        var root = new PopupMenu(this, nint.Zero, owner.Handler.MenuItems, null);
        _rootHWnd = root.HWnd;
        _ownerByHWnd[_rootHWnd] = nint.Zero;
        currentLeafHWnd = _rootHWnd;
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

        return Show(Owner, destroyOnClickOutside);
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
        var newLeaf = _ownerByHWnd.GetValueOrDefault(currentLeafHWnd, nint.Zero);
        PInvoke.PostMessage(currentLeafHWnd, PInvoke.WM_CLOSE, nint.Zero, nint.Zero);
        currentLeafHWnd = newLeaf;
    }

    /// <inheritdoc/>
    public void Dispose() => PInvoke.PostMessage(_rootHWnd, PInvoke.WM_CLOSE, nint.Zero, nint.Zero);

    internal List<Rectangle> GetOpenWindowRects(nint excludeHWnd)
    {
        var rects = new List<Rectangle>(_ownerByHWnd.Count);

        foreach (var hWnd in _ownerByHWnd.Keys)
        {
            if (hWnd == excludeHWnd) continue;
            if (!PInvoke.GetWindowRect(hWnd, out var rect)) continue;

            rects.Add(new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top));
        }

        return rects;
    }

    internal void OpenChild(nint ownerHWnd, MenuItemCollection items, Rectangle anchorScreenRect, bool selectFirstItem)
    {
        CloseChildrenOf(ownerHWnd);

        var child = new PopupMenu(this, ownerHWnd, items, anchorScreenRect, selectFirstItem);
        _ownerByHWnd[child.HWnd] = ownerHWnd;
        currentLeafHWnd = child.HWnd;
    }

    internal void CloseChildrenOf(nint hWnd)
    {
        while (currentLeafHWnd != hWnd && currentLeafHWnd != nint.Zero && _ownerByHWnd.ContainsKey(currentLeafHWnd))
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
        _ownerByHWnd.Remove(hWnd);

        if (hWnd == _rootHWnd) DisposeCore();
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
    /// <param name="owner">The owner of this tree</param>
    /// <param name="destroyOnClickOutside"><see langword="true"/> if this popup tree should be destroyed when clicked outside, otherwise <see langword="false"/></param>
    /// <returns><see cref="PopupMenuTree"/></returns>
    public static PopupMenuTree Show(NotifyIcon<DefaultPopupMenuHandler> owner, bool destroyOnClickOutside)
        => new PopupMenuTree(owner, destroyOnClickOutside);

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