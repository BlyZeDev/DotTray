namespace DotTray.Popup.Default;

using DotTray;
using DotTray.Internal;
using DotTray.Internal.Native;
using System;
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

        var current = currentLeafHWnd;

        while (current != nint.Zero)
        {
            if (PInvoke.GetWindowRect(current, out var rect) && PInvoke.PtInRect(ref rect, pos)) return true;

            current = PInvoke.GetWindow(current, PInvoke.GW_OWNER);
        }

        return false;
    }
}