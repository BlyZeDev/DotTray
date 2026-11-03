namespace DotTray.Direct.Internal.Models;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct DWriteFontMetrics
{
    public ushort DesignUnitsPerEm;
    public ushort Ascent;
    public ushort Descent;
    public short LineGap;
    public ushort CapHeight;
    public ushort XHeight;
    public short UnderlinePosition;
    public ushort UnderlineThickness;
    public short StrikethroughPosition;
    public ushort StrikethroughThickness;
}