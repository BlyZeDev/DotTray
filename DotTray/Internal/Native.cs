namespace DotTray.Internal;

using DotTray.Internal.Win32;
using System.Runtime.InteropServices;

internal static class Native
{
    private const string User32 = "user32.dll";
    private const string Kernel32 = "kernel32.dll";
    private const string Shell32 = "shell32.dll";

    public const int GWLP_USERDATA = -21;

    public const int WM_COMMAND = 0x0111;

    public const int WM_APP = 0x8000;
    public const uint WM_APP_TRAYICON = WM_APP + 1;
    public const uint WM_APP_TRAYICON_TOOLTIP = WM_APP + 2;
    public const uint WM_APP_TRAYICON_REBUILD = WM_APP + 3;
    public const uint WM_APP_TRAYICON_QUIT = WM_APP + 100;

    public const int WM_LBUTTONUP = 0x202;
    public const int WM_RBUTTONUP = 0x205;

    public const uint MF_STRING = 0x0000;
    public const uint MF_POPUP = 0x0010;
    public const uint MF_SEPARATOR = 0x0800;
    public const uint MF_GRAYED = 0x0001;
    public const uint MF_CHECKED = 0x0008;

    public const uint NIF_MESSAGE = 0x00000001;
    public const uint NIF_ICON = 0x00000002;
    public const uint NIF_TIP = 0x00000004;

    public const uint NIM_ADD = 0x00000000;
    public const uint NIM_MODIFY = 0x00000001;
    public const uint NIM_DELETE = 0x00000002;

    public const uint TPM_RIGHTBUTTON = 0x0002;

    public const int ID_TRAY_ICON = 1000;

    public const uint IMAGE_ICON = 1;
    public const uint LR_LOADFROMFILE = 0x00000010;

    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern nint LoadImage(nint hInst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    [DllImport(User32, SetLastError = true)]
    public static extern bool DestroyIcon(nint hIcon);

    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern ushort RegisterClass([In] ref WNDCLASS lpwc);

    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool UnregisterClass(string lpClassName, nint hInstance);

    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern nint CreateWindowEx(
        uint dwExStyle, string lpClassName, string lpWindowName,
        uint dwStyle, int x, int y, int nWidth, int nHeight,
        nint hWndParent, nint hMenu, nint hInstance, nint lpParam);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyWindow(nint hWnd);

    [DllImport(User32, SetLastError = true)]
    public static extern nint CreatePopupMenu();

    [DllImport(User32, SetLastError = true)]
    public static extern bool TrackPopupMenu(nint hMenu, uint flags, int x, int y, int r, nint hWnd, nint rect);

    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool AppendMenu(nint hMenu, uint uFlags, nint uIDNewItem, string lpNewItem);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyMenu(nint hMenu);

    [DllImport(User32, SetLastError = true)]
    public static extern nint DefWindowProc(nint hWnd, uint msg, nint wParam, nint lParam);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetMessage(out MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PostMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [DllImport(User32, SetLastError = true)]
    public static extern void PostQuitMessage(int nExitCode);

    [DllImport(User32, SetLastError = true)]
    public static extern nint SendMessage(nint hWnd, int Msg, nint wParam, nint lParam);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool TranslateMessage([In] ref MSG lpMsg);

    [DllImport(User32, SetLastError = true)]
    public static extern nint DispatchMessage([In] ref MSG lpmsg);

    [DllImport(User32, SetLastError = true)]
    public static extern bool GetCursorPos(out POINT pt);

    [DllImport(User32, SetLastError = true)]
    public static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

    [DllImport(User32, SetLastError = true)]
    public static extern bool SetForegroundWindow(nint hWnd);

    [DllImport(Kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern nint GetModuleHandle(string? moduleName);

    [DllImport(Shell32, CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool Shell_NotifyIcon(uint message, ref NOTIFYICONDATA data);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam);
}