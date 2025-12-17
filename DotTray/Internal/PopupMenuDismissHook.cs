namespace DotTray.Internal;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using System;
using System.Runtime.InteropServices;

internal sealed class PopupMenuDismissHook : IDisposable
{
    private readonly nint _rootHWnd;
    private readonly PInvoke.LowLevelMouseProc _hookProc;
    private readonly nint _hookHandle;

    public PopupMenuDismissHook(nint rootHWnd)
    {
        _rootHWnd = rootHWnd;
        _hookProc = new PInvoke.LowLevelMouseProc(LowLevelMouseProcFunc);

        _hookHandle = PInvoke.SetWindowsHookEx(PInvoke.WH_MOUSE_LL, Marshal.GetFunctionPointerForDelegate(_hookProc), nint.Zero, 0);
    }

    public void Dispose()
    {
        if (_hookHandle != nint.Zero) PInvoke.UnhookWindowsHookEx(_hookHandle);
    }

    private nint LowLevelMouseProcFunc(int nCode, nint wParam, nint lParam)
    {
        if (nCode == 0)
        {
            if (wParam is PInvoke.WM_LBUTTONUP or PInvoke.WM_RBUTTONUP or PInvoke.WM_MBUTTONUP)
            {
                var mousePos = Marshal.PtrToStructure<POINT>(lParam);
                var targetHWnd = PInvoke.WindowFromPoint(mousePos);

                if (!IsInHierarchy(targetHWnd)) PInvoke.PostMessage(_rootHWnd, PInvoke.WM_CLOSE, nint.Zero, nint.Zero);
            }
        }

        return PInvoke.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    private bool IsInHierarchy(nint targetHWnd)
    {
        var current = _rootHWnd;

        while (current != nint.Zero)
        {
            if (current == targetHWnd) return true;

            current = PInvoke.GetWindow(current, PInvoke.GW_CHILD);
        }

        return false;
    }
}