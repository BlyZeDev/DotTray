namespace DotTray.Internal.Native;

using System.Runtime.InteropServices;

internal static partial class PInvoke
{
    [LibraryImport(Shcore, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetDpiForMonitor(nint hmonitor, uint dpiType, out uint dpiX, out uint dpiY);
}