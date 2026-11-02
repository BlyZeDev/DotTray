namespace DotTray.Direct.Internal.Models;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct LinearGradientBrushProperties
{
    public PointF StartPoint;
    public PointF EndPoint;
}