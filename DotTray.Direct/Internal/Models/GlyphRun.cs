namespace DotTray.Direct.Internal.Models;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct GlyphRun
{
    public nint FontFace;
    public float FontEmSize;
    public uint GlyphCount;
    public ushort* GlyphIndices;
    public float* GlyphAdvances;
    public nint GlyphOffsets;
    public int IsSideways;
    public uint BidiLevel;
}