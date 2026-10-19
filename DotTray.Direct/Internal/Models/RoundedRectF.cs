namespace DotTray.Direct.Internal.Models;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct RoundedRectF
{
    public RECTF Rect;
    public float RadiusX, RadiusY;
}