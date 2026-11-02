namespace DotTray.Direct.Internal.Models;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct GradientStopF
{
    public float Position;
    public ColorF Color;
}