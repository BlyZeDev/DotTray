namespace DotTray.Direct.Internal.Models;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct RadialGradientBrushProperties
{
    public PointF Center;
    public PointF GradientOriginOffset;
    public float RadiusX;
    public float RadiusY;
}