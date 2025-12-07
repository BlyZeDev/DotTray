namespace DotTray.Internal;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using System.Runtime.InteropServices;

internal static class PopupMenuDismissHook
{
    private static nint leafWindow;
    private static nint hookHandle;
    private static PInvoke.LowLevelMouseProc? hookProc;

    public static void Register(nint hWnd)
    {
        if (hookHandle != nint.Zero) PInvoke.UnhookWindowsHookEx(hookHandle);

        leafWindow = hWnd;

        hookProc = new PInvoke.LowLevelMouseProc(LowLevelMouseProcFunc);
        hookHandle = PInvoke.SetWindowsHookEx(PInvoke.WH_MOUSE_LL, Marshal.GetFunctionPointerForDelegate(hookProc), nint.Zero, 0);
    }

    private static nint LowLevelMouseProcFunc(int nCode, nint wParam, nint lParam)
    {
        if (nCode == 0)
        {
            if (wParam is PInvoke.WM_LBUTTONDOWN or PInvoke.WM_RBUTTONDOWN or PInvoke.WM_MBUTTONDOWN)
            {
                var mousePos = Marshal.PtrToStructure<POINT>(lParam);
                var targetHWnd = PInvoke.WindowFromPoint(mousePos);

                if (!IsInHierarchy()) PInvoke.PostMessage(leafWindow, PInvoke.WM_APP_TRAYICON_DISMISS_POPUPMENU, nint.Zero, nint.Zero);
            }
        }

        return PInvoke.CallNextHookEx(hookHandle, nCode, wParam, lParam);
    }

    private static bool IsInHierarchy()
    {

    }
}