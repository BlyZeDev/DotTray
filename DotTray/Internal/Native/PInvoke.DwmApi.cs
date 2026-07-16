namespace DotTray.Internal.Native;

using DotTray.Internal.Win32;
using System.Runtime.InteropServices;

internal static partial class PInvoke
{
    [LibraryImport(DwmApi, SetLastError = true)]
    public static partial int DwmSetWindowAttribute(nint hwnd, int attr, ref int attrValue, int attrSize);

    [LibraryImport(DwmApi, SetLastError = true)]
    public static partial int DwmExtendFrameIntoClientArea(nint hWnd, ref MARGINS pMarInset);
}