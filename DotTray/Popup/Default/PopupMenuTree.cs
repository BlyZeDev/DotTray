namespace DotTray.Popup.Default;

using DotTray;
using DotTray.Internal;
using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

/// <summary>
/// Manages a popup menu tree
/// </summary>
public sealed class PopupMenuTree : IDisposable
{
    private readonly PInvoke.LowLevelMouseProc? _pHook;
    private readonly nint _hHook;

    private readonly nint _rootHWnd;

    private nint currentLeafHWnd;

    internal NotifyIcon<DefaultPopupMenuHandler> Owner { get; }

    private PopupMenuTree(NotifyIcon<DefaultPopupMenuHandler> owner, bool destroyOnClickOutside)
    {
        Owner = owner;

        if (destroyOnClickOutside)
        {
            _pHook = new PInvoke.LowLevelMouseProc(LowLevelMouseProcFunc);
            _hHook = PInvoke.SetWindowsHookEx(PInvoke.WH_MOUSE_LL, Marshal.GetFunctionPointerForDelegate(_pHook), nint.Zero, 0);
        }

        var root = new PopupMenu(this, nint.Zero);
        _rootHWnd = root.HWnd;
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
    /// Shows a popup
    /// </summary>
    /// <remarks>
    /// The shown popup will be the new leaf popup and is owned by the previous leaf popup
    /// </remarks>
    public void Show()
    {
        var leaf = new PopupMenu(this, currentLeafHWnd);
        currentLeafHWnd = leaf.HWnd;
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
        var newLeaf = PInvoke.GetWindow(currentLeafHWnd, PInvoke.GW_OWNER);
        PInvoke.PostMessage(currentLeafHWnd, PInvoke.WM_CLOSE, nint.Zero, nint.Zero);
        currentLeafHWnd = newLeaf;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_hHook != nint.Zero) PInvoke.UnhookWindowsHookEx(_hHook);
        PInvoke.PostMessage(_rootHWnd, PInvoke.WM_CLOSE, nint.Zero, nint.Zero);
    }

    /// <summary>
    /// Creates a popup window tree and shows the root popup
    /// </summary>
    /// <param name="owner">The owner of this tree</param>
    /// <param name="destroyOnClickOutside"><see langword="true"/> if this popup tree should be destroyed when clicked outside, otherwise <see langword="false"/></param>
    /// <returns><see cref="PopupMenuTree"/></returns>
    public static PopupMenuTree Show(NotifyIcon<DefaultPopupMenuHandler> owner, bool destroyOnClickOutside)
        => new PopupMenuTree(owner, destroyOnClickOutside);

    private nint LowLevelMouseProcFunc(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0 && wParam is PInvoke.WM_LBUTTONUP or PInvoke.WM_RBUTTONUP or PInvoke.WM_MBUTTONUP)
        {
            if (!IsInHierarchy()) Dispose();
        }

        return PInvoke.CallNextHookEx(_hHook, nCode, wParam, lParam);
    }

    private bool IsInHierarchy()
    {
        PInvoke.GetCursorPos(out var pos);

        foreach (var hWnd in EnumerateOwnerWindows(currentLeafHWnd, true))
        {
            if (IsHit(hWnd, pos)) return true;
        }

        return false;
    }

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