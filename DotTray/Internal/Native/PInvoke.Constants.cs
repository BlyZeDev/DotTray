namespace DotTray.Internal.Native;

internal static partial class PInvoke
{
    private const string User32 = "user32.dll";
    private const string Kernel32 = "kernel32.dll";
    private const string Shell32 = "shell32.dll";
    private const string Gdi32 = "gdi32.dll";
    private const string GdiPlus = "gdiplus.dll";
    private const string DwmApi = "dwmapi.dll";

    public const int GWLP_WNDPROC = -4;
    public const int GWLP_USERDATA = -21;

    public const uint WS_POPUP = 0x80000000;
    public const uint WS_BORDER = 0x00800000;
    public const uint WS_EX_NOACTIVATE = 0x08000000;
    public const uint WS_EX_TOOLWINDOW = 0x00000080;
    public const uint WS_EX_TOPMOST = 0x00000008;
    public const uint WS_VISIBLE = 0x10000000;
    public const uint WS_CLIPSIBLINGS = 0x04000000;
    public const uint WS_CLIPCHILDREN = 0x02000000;

    public const uint CS_HREDRAW = 0x0002;
    public const uint CS_VREDRAW = 0x0001;

    public const uint SPI_GETNONCLIENTMETRICS = 0x0029;

    public const int HTCLIENT = 1;
    public const int GA_ROOT = 2;
    public const int SRCCOPY = 0x00CC0020;

    public const int SW_SHOWNOACTIVATE = 4;

    public const uint WM_ERASEBKGND = 0x0014;
    public const uint WM_PAINT = 0x000F;

    public const uint WM_KILLFOCUS = 0x0008;
    public const uint WM_TIMER = 0x0113;
    public const uint WM_DESTROY = 0x0002;
    public const uint WM_COMMAND = 0x0111;
    public const uint WM_CLOSE = 0x0010;
    public const uint WM_QUIT = 0x0012;

    public const int WM_APP = 0x8000;
    public const uint WM_APP_TRAYICON = WM_APP + 1;
    public const uint WM_APP_TRAYICON_ICON = WM_APP + 2;
    public const uint WM_APP_TRAYICON_TOOLTIP = WM_APP + 3;
    public const uint WM_APP_TRAYICON_BALLOON = WM_APP + 4;

    public const uint WM_MEASUREITEM = 0x002C;
    public const uint WM_DRAWITEM = 0x002B;
    public const uint WM_DELETEITEM = 0x002D;

    public const int WM_MOUSEMOVE = 0x0200;
    public const int WM_LBUTTONDOWN = 0x0201;
    public const int WM_KEYDOWN = 0x0100;
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
    public const uint NIF_GUID = 0x00000020;

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
    public const uint DT_NOPREFIX = 0x0800;
    public const uint DT_END_ELLIPSIS = 0x8000;
    public const uint DT_CALCRECT = 0x0400;

    public const uint DFC_MENU = 2;

    public const uint DFCS_MENUCHECK = 0x0001;
    public const uint DFCS_FLAT = 0x4000;
    public const uint DFCS_MENUARROW = 0x0008;

    public const int SM_CXSCREEN = 0;
    public const int SM_CYSCREEN = 1;
    public const int SM_CXMENUCHECK = 71;
    public const int SM_CYMENUCHECK = 72;

    public const int COLOR_MENU = 4;
    public const int COLOR_MENUTEXT = 7;
    public const int COLOR_HIGHLIGHT = 13;
    public const int COLOR_HIGHLIGHTTEXT = 14;
    public const int COLOR_GRAYTEXT = 17;

    public const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    public const int DWMWA_SYSTEMBACKDROP_TYPE = 38;

    public const int DWMWCP_DEFAULT = 0;
    public const int DWMWCP_ROUND = 2;

    public const int DWMSBT_AUTO = 0;
    public const int DWMSBT_NONE = 1;
    public const int DWMSBT_MAINWINDOW = 2;
    public const int DWMSBT_TRANSIENTWINDOW = 3;
    public const int DWMSBT_TABBEDWINDOW = 4;

    public const uint TPM_RIGHTBUTTON = 0x0002;

    public const uint IMAGE_ICON = 1;
    public const uint LR_LOADFROMFILE = 0x00000010;

    public const uint SWP_NOACTIVATE = 0x0010;
    public const uint SWP_ZORDER = 0x0004;

    public const int VK_ESCAPE = 0x1B;

    public const int SmoothingModeInvalid = -1;
    public const int SmoothingModeDefault = 0;
    public const int SmoothingModeHighSpeed = 1;
    public const int SmoothingModeHighQuality = 2;
    public const int SmoothingModeNone = 3;
    public const int SmoothingModeAntiAlias = 4;
    public const int SmoothingModeAntiAlias8x8 = 5;

    public const int UnitWorld = 0;
    public const int UnitDisplay = 1;
    public const int UnitPixel = 2;
    public const int UnitPoint = 3;
    public const int UnitInch = 4;
    public const int UnitDocument = 5;
    public const int UnitMillimeter = 6;

    public const int AntiAliasGridFit = 4;

    public const int StringFormatFlagsNoWrap = 0x00001000;
}