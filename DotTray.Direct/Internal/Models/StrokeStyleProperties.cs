namespace DotTray.Direct.Internal.Models;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct StrokeStyleProperties
{
    public uint StartCap;
    public uint EndCap;
    public uint DashCap;
    public uint LineJoin;
    public float MiterLimit;
    public uint DashStyle;
    public float DashOffset;
}