namespace DotTray.Internal.Native;

internal static partial class PInvoke
{
    private const string User32 = "user32.dll";
    private const string Kernel32 = "kernel32.dll";
    private const string Shell32 = "shell32.dll";
    private const string Gdi32 = "gdi32.dll";
    private const string GdiPlus = "gdiplus.dll";
    private const string DwmApi = "dwmapi.dll";
    private const string Shcore = "shcore.dll";

    public const int GWLP_WNDPROC = -4;

    public const uint WS_EX_TOPMOST = 0x00000008;
    public const uint WS_EX_TOOLWINDOW = 0x00000080;
    public const uint WS_EX_NOACTIVATE = 0x08000000;

    public const uint WS_VISIBLE = 0x10000000;
    public const uint WS_CLIPCHILDREN = 0x02000000;
    public const uint WS_CLIPSIBLINGS = 0x04000000;
    public const uint WS_POPUP = 0x80000000;

    public const int WH_MOUSE_LL = 14;

    public const int IDC_ARROW = 32512;
    public const int IDC_HAND = 32649;

    public const int SRCCOPY = 0x00CC0020;

    public const int SW_SHOWNOACTIVATE = 4;

    public const uint WM_PAINT = 0x000F;

    public const uint WM_DESTROY = 0x0002;
    public const uint WM_CLOSE = 0x0010;

    public const int WM_APP = 0x8000;
    public const uint WM_APP_TRAYICON_CLICK = WM_APP + 1;
    public const uint WM_APP_TRAYICON_ICON = WM_APP + 2;
    public const uint WM_APP_TRAYICON_TOOLTIP = WM_APP + 3;
    public const uint WM_APP_TRAYICON_VISIBILITY = WM_APP + 4;
    public const uint WM_APP_TRAYICON_BALLOON = WM_APP + 5;
    public const uint WM_APP_TRAYICON_RESTART_SESSION = WM_APP + 6;

    public const int WM_MOUSEMOVE = 0x0200;
    public const int WM_MOUSELEAVE = 0x02A3;
    public const int WM_LBUTTONUP = 0x202;
    public const int WM_RBUTTONUP = 0x205;
    public const int WM_MBUTTONUP = 0x208;

    public const uint GW_OWNER = 4;

    public const int TME_LEAVE = 0x00000002;

    public const uint NIF_MESSAGE = 0x00000001;
    public const uint NIF_ICON = 0x00000002;
    public const uint NIF_TIP = 0x00000004;
    public const uint NIF_STATE = 0x00000008;
    public const uint NIF_INFO = 0x00000010;
    public const uint NIF_GUID = 0x00000020;

    public const uint NIS_HIDDEN = 0x00000001;

    public const uint NIIF_NONE = 0x00000000;
    public const uint NIIF_INFO = 0x00000001;
    public const uint NIIF_WARNING = 0x00000002;
    public const uint NIIF_ERROR = 0x00000003;
    public const uint NIIF_USER = 0x00000004;
    public const uint NIIF_NOSOUND = 0x00000010;

    public const uint NIM_ADD = 0x00000000;
    public const uint NIM_MODIFY = 0x00000001;
    public const uint NIM_DELETE = 0x00000002;

    public const uint MONITOR_DEFAULTTONEAREST = 2;

    public const uint MDT_EFFECTIVE_DPI = 0;

    public const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    public const int DWMWA_SYSTEMBACKDROP_TYPE = 38;

    public const int DWMWCP_ROUND = 2;

    public const int DWMSBT_TRANSIENTWINDOW = 2;

    public const int IMAGE_ICON = 1;
    public const int LR_LOADFROMFILE = 0x00000010;
    public const int LR_DEFAULTSIZE = 0x00000040;

    public const uint SWP_NOACTIVATE = 0x0010;
    public const uint SWP_ZORDER = 0x0004;

    public const nint DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4;

    public const int SmoothingModeAntiAlias8x8 = 5;

    public const int UnitPixel = 2;

    public const int StringFormatFlagsNoWrap = 0x00001000;

    public const int StringAlignmentNear = 0;
    public const int StringAlignmentCenter = 1;

    public const int Format32bppArgb = 0x26200A;
}