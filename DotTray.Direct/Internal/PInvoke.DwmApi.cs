namespace DotTray.Direct.Internal;

using System.Runtime.InteropServices;

internal static partial class PInvoke
{
    private const string DwmApi = "dwmapi.dll";

    public const uint DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    public const uint DWMWA_BORDER_COLOR = 34;

    public const int DWMWCP_ROUND = 2;
    public const uint DWMWA_COLOR_NONE = 0xFFFFFFFE;


    [LibraryImport(DwmApi, SetLastError = true)]
    public static partial int DwmSetWindowAttribute(nint hwnd, uint attr, ref int attrValue, int attrSize);

    [LibraryImport(DwmApi, SetLastError = true)]
    public static partial int DwmSetWindowAttribute(nint hwnd, uint attr, ref uint attrValue, int attrSize);
}