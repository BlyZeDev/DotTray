namespace DotTray.Direct.Internal;

using DotTray.Direct.Internal.COM;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

internal static partial class PInvoke
{
    private const string D2D1 = "d2d1.dll";

    public const uint D2D1_ALPHA_MODE_PREMULTIPLIED = 1;
    public const uint D2D1_BITMAP_OPTIONS_TARGET = 0x1;
    public const uint D2D1_BITMAP_OPTIONS_CANNOT_DRAW = 0x2;
    public const uint D2D1_ANTIALIAS_MODE_PER_PRIMITIVE = 0;
    public const uint D2D1_ANTIALIAS_MODE_ALIASED = 1;
    public const uint D2D1_TEXT_ANTIALIAS_MODE_GRAYSCALE = 2;
    public const uint D2D1_DRAW_TEXT_OPTIONS_NONE = 0;
    public const uint D2D1_DRAW_TEXT_OPTIONS_CLIP = 2;
    public const uint D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT = 4;
    public const uint D2D1_CAP_STYLE_ROUND = 2;
    public const uint D2D1_LINE_JOIN_ROUND = 2;
    public const uint D2D1_FIGURE_BEGIN_HOLLOW = 1;
    public const uint D2D1_FIGURE_END_OPEN = 0;
    public const uint D2D1_EXTEND_MODE_CLAMP = 0;
    public const uint D2D1_EXTEND_MODE_WRAP = 1;
    public const uint D2D1_EXTEND_MODE_MIRROR = 2;
    public const uint D2D1_INTERPOLATION_MODE_NEAREST_NEIGHBOR = 0;
    public const uint D2D1_INTERPOLATION_MODE_LINEAR = 1;
    public const uint D2D1_INTERPOLATION_MODE_CUBIC = 2;
    public const uint D2D1_INTERPOLATION_MODE_MULTI_SAMPLE_LINEAR = 3;
    public const uint D2D1_INTERPOLATION_MODE_ANISOTROPIC = 4;
    public const uint D2D1_INTERPOLATION_MODE_HIGH_QUALITY_CUBIC = 5;

    public const uint D2DERR_RECREATE_TARGET = 0x8899000C;

    [LibraryImport(D2D1)]
    public static partial int D2D1CreateDevice([MarshalUsing(typeof(ComInterfaceMarshaller<IDXGIDevice>))] IDXGIDevice dxgiDevice, nint creationProperties, [MarshalUsing(typeof(ComInterfaceMarshaller<ID2D1Device>))] out ID2D1Device device);
}