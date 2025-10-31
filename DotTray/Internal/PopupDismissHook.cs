namespace DotTray.Internal;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using System;
using System.Runtime.InteropServices;

internal sealed class PopupDismissHook : IDisposable
{
    private readonly nint _popupHWnd;
    private readonly nint _hookHandle;
    private readonly PInvoke.LowLevelMouseProc _hookProc;

    public event Action? ClickedOutside;

    public PopupDismissHook(nint popupHWnd)
    {
        _popupHWnd = popupHWnd;
        _hookProc = new PInvoke.LowLevelMouseProc(LowLevelMouseProcFunc);
        _hookHandle = PInvoke.SetWindowsHookEx(PInvoke.WH_MOUSE_LL, Marshal.GetFunctionPointerForDelegate(_hookProc), nint.Zero, 0);
    }

    public void Dispose() => PInvoke.UnhookWindowsHookEx(_hookHandle);

    private nint LowLevelMouseProcFunc(int nCode, nint wParam, nint lParam)
    {
        if (nCode == 0)
        {
            if (wParam == PInvoke.WM_LBUTTONDOWN)
            {
                var mousePos = Marshal.PtrToStructure<POINT>(lParam);

                if (_popupHWnd != PInvoke.WindowFromPoint(mousePos)) ClickedOutside?.Invoke();
            }
        }

        return PInvoke.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }
}