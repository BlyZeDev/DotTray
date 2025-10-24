namespace DotTray.Internal.Win32;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct RECTF
{
    public float X;
    public float Y;
    public float Width;
    public float Height;
}