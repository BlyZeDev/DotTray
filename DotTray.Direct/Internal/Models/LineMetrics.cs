namespace DotTray.Direct.Internal.Models;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct LineMetrics
{
    public uint Length;
    public uint TrailingWhitespaceLength;
    public uint NewlineLength;
    public float Height;
    public float Baseline;
    public int IsTrimmed;
}