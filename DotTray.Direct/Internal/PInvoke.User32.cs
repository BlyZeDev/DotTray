namespace DotTray.Direct.Internal;

using DotTray.Direct.Internal.Models;
using System.Runtime.InteropServices;

internal static partial class PInvoke
{
    private const string User32 = "user32.dll";

    public const int GWLP_USERDATA = -21;

    public const uint WS_CLIPCHILDREN = 0x02000000;
    public const uint WS_CLIPSIBLINGS = 0x04000000;
    public const uint WS_POPUP = 0x80000000;

    public const uint WS_EX_TOPMOST = 0x00000008;
    public const uint WS_EX_TOOLWINDOW = 0x00000080;
    public const uint WS_EX_NOACTIVATE = 0x08000000;
    public const uint WS_EX_NOREDIRECTIONBITMAP = 0x00200000;

    public const int SW_SHOW = 5;
    public const int SW_SHOWNOACTIVATE = 4;

    public const uint SWP_NOZORDER = 0x0004;
    public const uint SWP_NOACTIVATE = 0x0010;
    public const uint SWP_NOCOPYBITS = 0x0100;

    public const int WM_DESTROY = 0x0002;
    public const int WM_ACTIVATE = 0x0006;
    public const int WM_PAINT = 0x000F;
    public const int WM_CLOSE = 0x0010;
    public const int WM_ERASEBKGND = 0x0014;
    public const int WM_SETTINGCHANGE = 0x001A;
    public const int WM_CANCELMODE = 0x001F;
    public const int WM_SETCURSOR = 0x0020;
    public const int WM_DISPLAYCHANGE = 0x007E;
    public const int WM_NCCREATE = 0x0081;
    public const int WM_NCDESTROY = 0x0082;
    public const int WM_NCACTIVATE = 0x0086;
    public const int WM_KEYDOWN = 0x0100;
    public const int WM_SYSKEYDOWN = 0x0104;
    public const int WM_TIMER = 0x0113;
    public const int WM_MOUSEMOVE = 0x0200;
    public const int WM_LBUTTONDOWN = 0x0201;
    public const int WM_LBUTTONUP = 0x0202;
    public const int WM_RBUTTONDOWN = 0x0204;
    public const int WM_RBUTTONUP = 0x0205;
    public const int WM_MBUTTONDOWN = 0x0207;
    public const int WM_CAPTURECHANGED = 0x0215;
    public const int WM_DPICHANGED = 0x02E0;
    public const int WM_APP = 0x8000;

    public const int WA_INACTIVE = 0;

    public const uint EVENT_SYSTEM_MENUSTART = 0x0004;
    public const uint EVENT_SYSTEM_MENUEND = 0x0005;
    public const uint EVENT_SYSTEM_MENUPOPUPSTART = 0x0006;
    public const uint EVENT_SYSTEM_MENUPOPUPEND = 0x0007;
    public const int OBJID_CLIENT = -4;
    public const int CHILDID_SELF = 0;

    public const uint MONITOR_DEFAULTTONEAREST = 2;

    public const int VK_RETURN = 0x0D;
    public const int VK_MENU = 0x12;
    public const int VK_ESCAPE = 0x1B;
    public const int VK_SPACE = 0x20;
    public const int VK_END = 0x23;
    public const int VK_HOME = 0x24;
    public const int VK_LEFT = 0x25;
    public const int VK_UP = 0x26;
    public const int VK_RIGHT = 0x27;
    public const int VK_DOWN = 0x28;

    public const uint CS_DROPSHADOW = 0x00020000;

    public const nint SPI_SETWORKAREA = 0x002F;
    public const uint SPI_GETMENUSHOWDELAY = 0x006A;

    public static readonly nint HWND_MESSAGE = -3;
    public static readonly nint IDC_ARROW = 32512;
    public static readonly nint DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4;

    [LibraryImport(User32, EntryPoint = "RegisterClassExW", SetLastError = true)]
    public static partial ushort RegisterClassEx(ref WNDCLASSEX wndClass);

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
    public static partial bool SetForegroundWindow(nint hWnd);

    [LibraryImport(User32, SetLastError = true)]
    public static partial nint GetForegroundWindow();

    [LibraryImport(User32, SetLastError = true)]
    public static partial nint SetCapture(nint hWnd);

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ReleaseCapture();

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

    [LibraryImport(User32, EntryPoint = "LoadCursorW", SetLastError = true)]
    public static partial nint LoadCursor(nint hInstance, nint cursorName);

    [LibraryImport(User32, SetLastError = true)]
    public static partial nint SetCursor(nint hCursor);

    [LibraryImport(User32, SetLastError = true)]
    public static partial void NotifyWinEvent(uint winEvent, nint hWnd, int idObject, int idChild);

    [LibraryImport(User32, SetLastError = true)]
    public static partial uint GetDpiForWindow(nint hWnd);

    [LibraryImport(User32, SetLastError = true)]
    public static partial nint SetThreadDpiAwarenessContext(nint dpiContext);

    [LibraryImport(User32, EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    public static partial nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

    [LibraryImport(User32, EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    public static partial nint GetWindowLongPtr(nint hWnd, int nIndex);

    [LibraryImport(User32, SetLastError = true)]
    public static partial nint MonitorFromPoint(POINT pt, uint dwFlags);

    [LibraryImport(User32, EntryPoint = "GetMonitorInfoW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetMonitorInfo(nint hMonitor, ref MONITORINFO lpmi);

    [LibraryImport(User32, SetLastError = true)]
    public static partial nint SetTimer(nint hWnd, nint nIDEvent, uint uElapse, nint lpTimerFunc);

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool KillTimer(nint hWnd, nint uIDEvent);

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool InvalidateRect(nint hWnd, nint lpRect, [MarshalAs(UnmanagedType.Bool)] bool bErase);

    [LibraryImport(User32, SetLastError = true)]
    public static partial nint BeginPaint(nint hWnd, out PAINTSTRUCT lpPaint);

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool EndPaint(nint hWnd, ref PAINTSTRUCT lpPaint);

    [LibraryImport(User32, EntryPoint = "SystemParametersInfoW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SystemParametersInfo(uint uiAction, uint uiParam, out uint pvParam, uint fWinIni);
}