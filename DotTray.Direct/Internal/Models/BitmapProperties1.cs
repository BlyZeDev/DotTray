namespace DotTray.Direct.Internal.Models;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct BitmapProperties1
{
    public uint Format;
    public uint AlphaMode;
    public float DpiX, DpiY;
    public uint Options;
    public nint ColorContext;
}