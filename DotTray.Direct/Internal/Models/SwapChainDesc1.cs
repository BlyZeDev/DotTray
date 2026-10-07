namespace DotTray.Direct.Internal.Models;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct SwapChainDesc1
{
    public uint Width, Height, Format;
    public int Stereo;
    public SampleDesc SampleDesc;
    public uint BufferUsage, BufferCount, Scaling, SwapEffect, AlphaMode, Flags;
}