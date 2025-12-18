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

    public nint LeafHWnd { get; set; }

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
        if (nCode >= 0)
        {
            if (wParam is PInvoke.WM_LBUTTONUP or PInvoke.WM_RBUTTONUP or PInvoke.WM_MBUTTONUP)
            {
                var mousePos = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam).pt;

                if (!IsInHierarchy(mousePos)) PInvoke.PostMessage(_rootHWnd, PInvoke.WM_CLOSE, nint.Zero, nint.Zero);
            }
        }

        return PInvoke.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    private bool IsInHierarchy(POINT mousePos)
    {
        var current = LeafHWnd;
        Console.WriteLine(current);

        while (current != nint.Zero)
        {
            if (PInvoke.GetWindowRect(current, out var rect))
            {
                //Debug
                Console.WriteLine($"Rect: {rect.Left},{rect.Top},{rect.Right},{rect.Bottom}");
                Console.WriteLine($"Mouse: {mousePos.x},{mousePos.y}");

                if (PInvoke.PtInRect(ref rect, mousePos)) return true;
            }

            current = PInvoke.GetWindow(current, PInvoke.GW_OWNER);
        }

        //Debug
        Console.WriteLine("FALSE");

        return false;
    }
}