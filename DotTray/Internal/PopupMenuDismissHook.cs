namespace DotTray.Internal;

using DotTray.Internal.Native;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[SupportedOSPlatform("windows")]
internal sealed class PopupMenuDismissHook : IDisposable
{
    private readonly PopupMenuSession _session;
    private readonly PInvoke.LowLevelMouseProc _hookProc;
    private readonly nint _hookHandle;

    public nint LeafHWnd { get; set; }

    public PopupMenuDismissHook(PopupMenuSession session)
    {
        _session = session;
        _hookProc = new PInvoke.LowLevelMouseProc(LowLevelMouseProcFunc);

        _hookHandle = PInvoke.SetWindowsHookEx(PInvoke.WH_MOUSE_LL, Marshal.GetFunctionPointerForDelegate(_hookProc), nint.Zero, 0);
    }

    public void Dispose()
    {
        if (_hookHandle != nint.Zero) PInvoke.UnhookWindowsHookEx(_hookHandle);
    }

    private nint LowLevelMouseProcFunc(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0 && wParam is PInvoke.WM_LBUTTONUP or PInvoke.WM_RBUTTONUP or PInvoke.WM_MBUTTONUP)
        {
            if (!IsInHierarchy()) _session.Dispose();
        }

        return PInvoke.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    private bool IsInHierarchy()
    {
        PInvoke.GetCursorPos(out var mousePos);

        var current = LeafHWnd;

        while (current != nint.Zero)
        {
            if (PInvoke.GetWindowRect(current, out var rect) && PInvoke.PtInRect(ref rect, mousePos)) return true;

            current = PInvoke.GetWindow(current, PInvoke.GW_OWNER);
        }

        return false;
    }
}