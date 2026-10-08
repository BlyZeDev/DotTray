namespace DotTray.Direct.Internal;

using DotTray.Direct.Internal.Models;
using System.Runtime.InteropServices;

internal static partial class PInvoke
{
    private const string User32 = "user32.dll";

    public const int GWLP_WNDPROC = -4;

    public const uint WS_CLIPCHILDREN = 0x02000000;
    public const uint WS_CLIPSIBLINGS = 0x04000000;
    public const uint WS_POPUP = 0x80000000;

    public const uint WS_EX_TOPMOST = 0x00000008;
    public const uint WS_EX_TOOLWINDOW = 0x00000080;
    public const uint WS_EX_NOACTIVATE = 0x08000000;
    public const uint WS_EX_NOREDIRECTIONBITMAP = 0x00200000;

    public const int SW_HIDE = 0;
    public const int SW_SHOW = 5;
    public const int SW_SHOWNOACTIVATE = 4;

    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_NOMOVE = 0x0002;
    public const uint SWP_NOZORDER = 0x0004;
    public const uint SWP_NOACTIVATE = 0x0010;

    public const uint WM_DESTROY = 0x0002;
    public const uint WM_ACTIVATE = 0x0006;
    public const uint WM_PAINT = 0x000F;
    public const uint WM_CLOSE = 0x0010;
    public const uint WM_ERASEBKGND = 0x0014;
    public const uint WM_KEYDOWN = 0x0100;
    public const uint WM_MOUSEMOVE = 0x0200;
    public const uint WM_LBUTTONDOWN = 0x0201;
    public const uint WM_LBUTTONUP = 0x0202;
    public const uint WM_MOUSELEAVE = 0x02A3;
    public const uint WM_DPICHANGED = 0x02E0;
    public const uint WM_APP = 0x8000;

    public const int VK_ESCAPE = 0x1B;
    public const int WA_INACTIVE = 0;

    public static readonly nint HWND_TOPMOST = -1;
    public static readonly nint IDC_ARROW = 32512;
    public static readonly nint DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4;

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam);

    [LibraryImport(User32, EntryPoint = "RegisterClassExW", SetLastError = true)]
    public static partial ushort RegisterClassEx(ref WNDCLASSEX wndClass);

    [LibraryImport(User32, EntryPoint = "UnregisterClassW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool UnregisterClass(nint className, nint hInstance);

    [LibraryImport(User32, EntryPoint = "CreateWindowExW", SetLastError = true)]
    public static partial nint CreateWindowEx(uint exStyle, nint className, nint windowName, uint style, int x, int y, int width, int height, nint parent, nint menu, nint hInstance, nint param);

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyWindow(nint hWnd);

    [LibraryImport(User32, EntryPoint = "DefWindowProcW", SetLastError = true)]
    public static partial nint DefWindowProc(nint hWnd, uint msg, nint wParam, nint lParam);

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ShowWindow(nint hWnd, int cmdShow);

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ValidateRect(nint hWnd, nint rect);

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetForegroundWindow(nint hWnd);

    [LibraryImport(User32, EntryPoint = "GetMessageW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetMessage(out MSG message, nint hWnd, uint filterMin, uint filterMax);

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool TranslateMessage(ref MSG message);

    [LibraryImport(User32, EntryPoint = "DispatchMessageW", SetLastError = true)]
    public static partial nint DispatchMessage(ref MSG message);

    [LibraryImport(User32, EntryPoint = "PostMessageW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool PostMessage(nint hWnd, uint msg, nint wParam, nint lParam);

    [LibraryImport(User32, SetLastError = true)]
    public static partial void PostQuitMessage(int exitCode);

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetCursorPos(out POINT point);

    [LibraryImport(User32, EntryPoint = "LoadCursorW", SetLastError = true)]
    public static partial nint LoadCursor(nint hInstance, nint cursorName);

    [LibraryImport(User32, SetLastError = true)]
    public static partial uint GetDpiForWindow(nint hWnd);

    [LibraryImport(User32, SetLastError = true)]
    public static partial nint SetThreadDpiAwarenessContext(nint dpiContext);

    [LibraryImport(User32, EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    public static partial nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);
}