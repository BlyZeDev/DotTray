namespace DotTray.Internal.Win32;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct ICONINFO
{
    public int fIcon;

    public uint xHotspot;
    public uint yHotspot;

    public nint hbmMask;
    public nint hbmColor;
}