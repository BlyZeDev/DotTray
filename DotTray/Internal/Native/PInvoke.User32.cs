namespace DotTray.Internal.Native;

using DotTray.Internal.Win32;
using System;
using System.Runtime.InteropServices;

internal static partial class PInvoke
{
    [DllImport(User32, SetLastError = true)]
    public static extern int GetSystemMetrics(int nIndex);

    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern nint LoadImage(nint hInst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyIcon(nint hIcon);

    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern ushort RegisterClass([In] ref WNDCLASS lpwc);

    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnregisterClass(string lpClassName, nint hInstance);

    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern nint CreateWindowEx(
        uint dwExStyle, string lpClassName, string lpWindowName,
        uint dwStyle, int x, int y, int nWidth, int nHeight,
        nint hWndParent, nint hMenu, nint hInstance, nint lpParam);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShowWindow(nint hWnd, int nCmdShow);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UpdateWindow(nint hWnd);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyWindow(nint hWnd);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool InvalidateRect(nint hWnd, nint lpRect, bool bErase);

    [DllImport(User32, SetLastError = true)]
    public static extern nint GetDesktopWindow();

    [DllImport(User32, SetLastError = true)]
    public static extern nint SetActiveWindow(nint hWnd);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool AdjustWindowRectEx(ref RECT lpRect, uint dwStyle, [MarshalAs(UnmanagedType.Bool)] bool bMenu, uint dwExStyle);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetCapture(nint hWnd);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ReleaseCapture();

    [DllImport(User32, SetLastError = true)]
    public static extern nint CreatePopupMenu();

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool TrackPopupMenu(nint hMenu, uint flags, int x, int y, int r, nint hWnd, nint rect);

    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool AppendMenu(nint hMenu, uint uFlags, nint uIDNewItem, string? lpNewItem);

    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetMenuItemInfo(nint hMenu, uint uItem, bool fByPosition, ref MENUITEMINFO lpmii);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyMenu(nint hMenu);

    [DllImport(User32, SetLastError = true)]
    public static extern nint DefWindowProc(nint hWnd, uint msg, nint wParam, nint lParam);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PeekMessage(out MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetMessage(out MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PostMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [DllImport(User32, SetLastError = true)]
    public static extern void PostQuitMessage(int nExitCode);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool TranslateMessage([In] ref MSG lpMsg);

    [DllImport(User32, SetLastError = true)]
    public static extern nint DispatchMessage([In] ref MSG lpmsg);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out POINT pt);

    [DllImport(User32, SetLastError = true)]
    public static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(nint hWnd);

    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int DrawText(nint hdc, string lpString, int nCount, ref RECT lpRect, uint uFormat);

    [DllImport(User32, SetLastError = true)]
    public static extern nint GetSysColorBrush(int nIndex);

    [DllImport(User32, SetLastError = true)]
    public static extern int GetSysColor(int nIndex);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetClientRect(nint hWnd, out RECT lpRect);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ClientToScreen(nint hWnd, ref POINT lpPoint);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FillRect(nint hdc, ref RECT lprc, nint hbr);

    [DllImport(User32, SetLastError = true)]
    public static extern nint GetDC(nint hWnd);

    [DllImport(User32, SetLastError = true)]
    public static extern int ReleaseDC(nint hWnd, nint hDC);

    [DllImport(User32, SetLastError = true)]
    public static extern nint BeginPaint(nint hWnd, out PAINTSTRUCT lpPaint);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EndPaint(nint hWnd, ref PAINTSTRUCT lpPaint);

    [DllImport(User32, SetLastError = true)]
    public static extern short GetAsyncKeyState(int vKey);
}