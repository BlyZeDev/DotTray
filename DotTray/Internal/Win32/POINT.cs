namespace DotTray.Internal.Win32;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct POINT
{
    public int x;
    public int y;
}