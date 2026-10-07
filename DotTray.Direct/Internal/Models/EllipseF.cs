namespace DotTray.Direct.Internal.Models;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct EllipseF
{
    public float X, Y, RadiusX, RadiusY;
}