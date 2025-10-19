namespace DotTray.Internal.Native;

using System.Runtime.InteropServices;

internal static partial class PInvoke
{
    [DllImport(DwmApi, SetLastError = true)]
    public static extern int DwmSetWindowAttribute(nint hwnd, int attr, ref int attrValue, int attrSize);
}