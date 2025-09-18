namespace DotTray.Internal;

using DotTray.Internal.Win32;
using System;
using System.Runtime.InteropServices;

internal static unsafe class Native
{
    private const string User32 = "user32.dll";
    private const string Kernel32 = "kernel32.dll";
    private const string Shell32 = "shell32.dll";
    private const string Gdi32 = "gdi32.dll";
    private const string GdiPlus = "gdiplus.dll";

    public const int GWLP_USERDATA = -21;

    public const int WM_COMMAND = 0x0111;

    public const int WM_APP = 0x8000;
    public const uint WM_APP_TRAYICON = WM_APP + 1;
    public const uint WM_APP_TRAYICON_ICON = WM_APP + 2;
    public const uint WM_APP_TRAYICON_TOOLTIP = WM_APP + 3;
    public const uint WM_APP_TRAYICON_BALLOON = WM_APP + 4;
    public const uint WM_APP_TRAYICON_REBUILD = WM_APP + 99;
    public const uint WM_APP_TRAYICON_QUIT = WM_APP + 100;

    public const uint WM_MEASUREITEM = 0x002C;
    public const uint WM_DRAWITEM = 0x002B;
    public const uint WM_DELETEITEM = 0x002D;

    public const int WM_LBUTTONUP = 0x202;
    public const int WM_RBUTTONUP = 0x205;
    public const int WM_MBUTTONUP = 0x208;

    public const uint MF_STRING = 0x0000;
    public const uint MF_POPUP = 0x0010;
    public const uint MF_SEPARATOR = 0x0800;
    public const uint MF_GRAYED = 0x0001;
    public const uint MF_CHECKED = 0x0008;
    public const uint MF_OWNERDRAW = 0x0100;

    public const uint MFS_CHECKED = 0x0008;
    public const uint MFS_DISABLED = 0x0003;

    public const int ODS_SELECTED = 0x0001;
    public const int ODS_GRAYED = 0x0002;
    public const int ODS_DISABLED = 0x0004;
    public const int ODS_CHECKED = 0x0008;
    public const int ODS_FOCUS = 0x0010;
    public const int ODS_DEFAULT = 0x0020;
    public const int ODS_HOTLIGHT = 0x0040;
    public const int ODS_INACTIVE = 0x0080;
    public const int ODS_NOACCEL = 0x0100;
    public const int ODS_NOFOCUSRECT = 0x0200;

    public const uint NIF_MESSAGE = 0x00000001;
    public const uint NIF_ICON = 0x00000002;
    public const uint NIF_TIP = 0x00000004;
    public const uint NIF_INFO = 0x00000010;

    public const uint NIIF_NONE = 0x00000000;
    public const uint NIIF_INFO = 0x00000001;
    public const uint NIIF_WARNING = 0x00000002;
    public const uint NIIF_ERROR = 0x00000003;
    public const uint NIIF_USER = 0x00000004;
    public const uint NIIF_NOSOUND = 0x00000010;

    public const uint MIIM_STATE = 0x00000001;
    public const uint MIIM_SUBMENU = 0x00000004;
    public const uint MIIM_DATA = 0x00000020;

    public const uint NIM_ADD = 0x00000000;
    public const uint NIM_MODIFY = 0x00000001;
    public const uint NIM_DELETE = 0x00000002;

    public const int PS_SOLID = 0;
    public const int TRANSPARENT = 1;
    public const int NULL_BRUSH = 5;
    public const int NULL_PEN = 8;

    public const uint DT_LEFT = 0x0000;
    public const uint DT_CENTER = 0x0001;
    public const uint DT_RIGHT = 0x0002;
    public const uint DT_VCENTER = 0x0004;
    public const uint DT_SINGLELINE = 0x0020;
    public const uint DT_NOPREFIX = 0x800;
    public const uint DT_END_ELLIPSIS = 0x8000;

    public const uint DFC_MENU = 2;

    public const uint DFCS_MENUCHECK = 0x0001;
    public const uint DFCS_FLAT = 0x4000;
    public const uint DFCS_MENUARROW = 0x0008;

    public const int SM_CXMENUCHECK = 71;
    public const int SM_CYMENUCHECK = 72;

    public const int COLOR_MENU = 4;
    public const int COLOR_MENUTEXT = 7;
    public const int COLOR_HIGHLIGHT = 13;
    public const int COLOR_HIGHLIGHTTEXT = 14;
    public const int COLOR_GRAYTEXT = 17;

    public const uint TPM_RIGHTBUTTON = 0x0002;

    public const uint IMAGE_ICON = 1;
    public const uint LR_LOADFROMFILE = 0x00000010;

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
    public static extern bool DestroyWindow(nint hWnd);

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

    [DllImport(Kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern nint GetModuleHandle(string? moduleName);

    [DllImport(Shell32, CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool Shell_NotifyIcon(uint message, ref NOTIFYICONDATA data);

    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int DrawText(nint hdc, string lpString, int nCount, ref RECT lpRect, uint uFormat);

    [DllImport(User32, SetLastError = true)]
    public static extern nint GetSysColorBrush(int nIndex);

    [DllImport(User32, SetLastError = true)]
    public static extern int GetSysColor(int nIndex);

    [DllImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FillRect(nint hdc, ref RECT lprc, nint hbr);

    [DllImport(Gdi32, SetLastError = true)]
    public static extern uint SetTextColor(nint hdc, int crColor);

    [DllImport(Gdi32, SetLastError = true)]
    public static extern int SetBkMode(nint hdc, int mode);

    [DllImport(User32, SetLastError = true)]
    public static extern nint GetDC(nint hWnd);

    [DllImport(User32, SetLastError = true)]
    public static extern int ReleaseDC(nint hWnd, nint hDC);

    [DllImport(Gdi32, CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetTextExtentPoint32(nint hdc, string lpString, int c, out SIZE psizl);

    [DllImport(Gdi32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool Polyline(nint hdc, POINT* lppt, int cPoints);

    [DllImport(Gdi32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool RoundRect(nint hdc, int left, int top, int right, int bottom, int width, int height);

    [DllImport(Gdi32, SetLastError = true)]
    public static extern nint CreateSolidBrush(int color);

    [DllImport(Gdi32, SetLastError = true)]
    public static extern nint CreatePen(int fnPenStyle, int nWidth, int crColor);

    [DllImport(Gdi32, SetLastError = true)]
    public static extern nint SelectObject(nint hdc, nint h);

    [DllImport(Gdi32, SetLastError = true)]
    public static extern nint GetStockObject(int fnObject);

    [DllImport(Gdi32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeleteObject(nint hObject);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdiplusStartup(out nint token, ref GDIPLUSSTARTUPINPUT input, out nint output);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern void GdiplusShutdown(nint token);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipCreateFromHDC(nint hdc, out nint graphics);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipDeleteGraphics(nint graphics);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipSetSmoothingMode(nint graphics, int smoothingMode);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipCreatePen1(uint color, float width, int unit, out nint pen);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipDeletePen(nint pen);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipDrawLinesI(nint graphics, nint pen, POINT* points, int count);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipSetPenLineJoin(nint pen, int lineJoin);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipSetPenStartCap(nint pen, int lineCap);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipSetPenEndCap(nint pen, int lineCap);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam);
}