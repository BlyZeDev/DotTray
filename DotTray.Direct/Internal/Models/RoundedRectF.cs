namespace DotTray.Direct.Internal.Models;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct RoundedRectF
{
    public RectF Rect;
    public float RadiusX, RadiusY;
}