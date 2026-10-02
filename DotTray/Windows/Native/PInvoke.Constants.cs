namespace DotTray.Windows.Native;

internal static partial class PInvoke
{
    private const string User32 = "user32.dll";
    private const string Shell32 = "shell32.dll";
    private const string UxTheme = "uxtheme.dll";
    private const string DwmApi = "dwmapi.dll";

    public const uint PM_REMOVE = 0x0001;

    public const uint WM_DESTROY = 0x0002;
    public const uint WM_CLOSE = 0x0010;

    public const int WM_APP = 0x8000;

    public const int WM_MOUSEMOVE = 0x0200;
    public const int WM_LBUTTONDOWN = 0x0201;
    public const int WM_LBUTTONUP = 0x0202;
    public const int WM_LBUTTONDBLCLK = 0x0203;
    public const int WM_RBUTTONDOWN = 0x0204;
    public const int WM_RBUTTONUP = 0x0205;
    public const int WM_MBUTTONDOWN = 0x0207;
    public const int WM_MBUTTONUP = 0x0208;
    public const int WM_CONTEXTMENU = 0x007B;

    public const uint WM_POWERBROADCAST = 0x0218;
    public const nint PBT_APMRESUMESUSPEND = 0x0007;
    public const nint PBT_APMRESUMEAUTOMATIC = 0x0012;

    public const int NIN_SELECT = 0x0400;
    public const int NIN_KEYSELECT = 0x0401;
    public const int NIN_BALLOONSHOW = 0x0402;
    public const int NIN_BALLOONHIDE = 0x0403;
    public const int NIN_BALLOONTIMEOUT = 0x0404;
    public const int NIN_BALLOONUSERCLICK = 0x0405;
    public const int NIN_POPUPOPEN = 0x0406;
    public const int NIN_POPUPCLOSE = 0x0407;

    public const uint MF_STRING = 0x00000000;
    public const uint MF_GRAYED = 0x00000001;
    public const uint MF_CHECKED = 0x00000008;
    public const uint MF_POPUP = 0x00000010;
    public const uint MF_SEPARATOR = 0x00000800;

    public const uint TPM_LEFTALIGN = 0x0000;
    public const uint TPM_BOTTOMALIGN = 0x0020;
    public const uint TPM_RETURNCMD = 0x0100;
    public const uint TPM_NONOTIFY = 0x0080;

    public const uint WM_NULL = 0x0000;

    public const uint NIF_MESSAGE = 0x00000001;
    public const uint NIF_ICON = 0x00000002;
    public const uint NIF_TIP = 0x00000004;
    public const uint NIF_STATE = 0x00000008;
    public const uint NIF_INFO = 0x00000010;
    public const uint NIF_GUID = 0x00000020;
    public const uint NIF_SHOWTIP = 0x00000080;

    public const uint NIS_HIDDEN = 0x00000001;

    public const uint NIIF_NONE = 0x00000000;
    public const uint NIIF_INFO = 0x00000001;
    public const uint NIIF_WARNING = 0x00000002;
    public const uint NIIF_ERROR = 0x00000003;
    public const uint NIIF_USER = 0x00000004;
    public const uint NIIF_NOSOUND = 0x00000010;
    public const uint NIIF_LARGE_ICON = 0x00000020;

    public const uint NIM_ADD = 0x00000000;
    public const uint NIM_MODIFY = 0x00000001;
    public const uint NIM_DELETE = 0x00000002;
    public const uint NIM_SETVERSION = 0x00000004;

    public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    public const int IMAGE_ICON = 1;
    public const int LR_LOADFROMFILE = 0x00000010;
    public const int LR_DEFAULTSIZE = 0x00000040;

    public const nint DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4;
}