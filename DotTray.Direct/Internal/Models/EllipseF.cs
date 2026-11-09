namespace DotTray.Direct.Internal.Models;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct EllipseF
{
    public PointF Point;
    public float RadiusX;
    public float RadiusY;
}