namespace DotTray.Windows.Native;

using DotTray.Windows.Native.Models;
using System.Runtime.InteropServices;

internal static partial class PInvoke
{
    [LibraryImport(User32, EntryPoint = "LoadImageW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial nint LoadImage(nint hInst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    [LibraryImport(User32, SetLastError = true)]
    public static partial nint CopyIcon(nint hIcon);

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyIcon(nint hIcon);

    [LibraryImport(User32, EntryPoint = "RegisterClassW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial ushort RegisterClass(ref WNDCLASS lpwc);

    [LibraryImport(User32, EntryPoint = "UnregisterClassW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool UnregisterClass(nint lpClassName, nint hInstance);

    [LibraryImport(User32, EntryPoint = "CreateWindowExW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial nint CreateWindowEx(
        uint dwExStyle, nint lpClassName, nint lpWindowName,
        uint dwStyle, int x, int y, int nWidth, int nHeight,
        nint hWndParent, nint hMenu, nint hInstance, nint lpParam);

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyWindow(nint hWnd);

    [LibraryImport(User32, EntryPoint = "DefWindowProcW", SetLastError = true)]
    public static partial nint DefWindowProc(nint hWnd, uint msg, nint wParam, nint lParam);

    [LibraryImport(User32, EntryPoint = "RegisterWindowMessageW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    public static partial uint RegisterWindowMessage(string lpString);

    [LibraryImport(User32, EntryPoint = "GetMessageW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetMessage(out MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [LibraryImport(User32, EntryPoint = "PeekMessageW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool PeekMessage(out MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint bRemoveMsg);

    [LibraryImport(User32, EntryPoint = "PostMessageW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool PostMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [LibraryImport(User32, SetLastError = true)]
    public static partial void PostQuitMessage(int nExitCode);

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool TranslateMessage(ref MSG lpMsg);

    [LibraryImport(User32, EntryPoint = "DispatchMessageW", SetLastError = true)]
    public static partial nint DispatchMessage(ref MSG lpmsg);

    [LibraryImport(User32, SetLastError = true)]
    public static partial nint SetThreadDpiAwarenessContext(nint dpiContext);

    [LibraryImport(User32, SetLastError = true)]
    public static partial nint CreatePopupMenu();

    [LibraryImport(User32, EntryPoint = "AppendMenuW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AppendMenu(nint hMenu, uint Flags, nuint uIDNewItem, string? lpNewItem);

    [LibraryImport(User32, SetLastError = true)]
    public static partial int TrackPopupMenuEx(nint hMenu, uint uFlags, int x, int y, nint hWnd, nint lptpm);

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool EndMenu();

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyMenu(nint hMenu);

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetForegroundWindow(nint hWnd);
}