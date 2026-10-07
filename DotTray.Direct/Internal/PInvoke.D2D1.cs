namespace DotTray.Direct.Internal;

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

internal static partial class PInvoke
{
    private const string D2D1 = "d2d1.dll";

    public const uint D2D1_ALPHA_MODE_PREMULTIPLIED = 1;
    public const uint D2D1_BITMAP_OPTIONS_TARGET = 0x1;
    public const uint D2D1_BITMAP_OPTIONS_CANNOT_DRAW = 0x2;
    public const uint D2D1_ANTIALIAS_MODE_PER_PRIMITIVE = 0;
    public const uint D2D1_TEXT_ANTIALIAS_MODE_GRAYSCALE = 2;
    public const uint D2D1_DRAW_TEXT_OPTIONS_NONE = 0;
    public const uint D2D1_DRAW_TEXT_OPTIONS_CLIP = 2;
    public const uint D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT = 4;

    [LibraryImport(D2D1)]
    public static partial int D2D1CreateDevice([MarshalUsing(typeof(ComInterfaceMarshaller<COM.IDXGIDevice>))] COM.IDXGIDevice dxgiDevice, nint creationProperties, [MarshalUsing(typeof(ComInterfaceMarshaller<COM.ID2D1Device>))] out COM.ID2D1Device device);
}