namespace DotTray.Direct.Internal;

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

internal static partial class PInvoke
{
    private const string DWRITE = "dwrite.dll";

    public const uint DWRITE_MEASURING_MODE_NATURAL = 0;
    public const uint DWRITE_TEXT_ALIGNMENT_LEADING = 0;
    public const uint DWRITE_TEXT_ALIGNMENT_TRAILING = 1;
    public const uint DWRITE_TEXT_ALIGNMENT_CENTER = 2;
    public const uint DWRITE_PARAGRAPH_ALIGNMENT_NEAR = 0;
    public const uint DWRITE_PARAGRAPH_ALIGNMENT_CENTER = 2;
    public const uint DWRITE_WORD_WRAPPING_WRAP = 0;
    public const uint DWRITE_WORD_WRAPPING_NO_WRAP = 1;

    public const int DWRITE_FONT_WEIGHT_MIN = 1;
    public const int DWRITE_FONT_WEIGHT_THIN = 100;
    public const int DWRITE_FONT_WEIGHT_EXTRA_LIGHT = 200;
    public const int DWRITE_FONT_WEIGHT_ULTRA_LIGHT = 200;
    public const int DWRITE_FONT_WEIGHT_LIGHT = 300;
    public const int DWRITE_FONT_WEIGHT_SEMI_LIGHT = 350;
    public const int DWRITE_FONT_WEIGHT_NORMAL = 400;
    public const int DWRITE_FONT_WEIGHT_REGULAR = 400;
    public const int DWRITE_FONT_WEIGHT_MEDIUM = 500;
    public const int DWRITE_FONT_WEIGHT_DEMI_BOLD = 600;
    public const int DWRITE_FONT_WEIGHT_SEMI_BOLD = 600;
    public const int DWRITE_FONT_WEIGHT_BOLD = 700;
    public const int DWRITE_FONT_WEIGHT_EXTRA_BOLD = 800;
    public const int DWRITE_FONT_WEIGHT_ULTRA_BOLD = 800;
    public const int DWRITE_FONT_WEIGHT_BLACK = 900;
    public const int DWRITE_FONT_WEIGHT_HEAVY = 900;
    public const int DWRITE_FONT_WEIGHT_EXTRA_BLACK = 950;
    public const int DWRITE_FONT_WEIGHT_ULTRA_BLACK = 950;
    public const int DWRITE_FONT_WEIGHT_MAX = 999;

    public const int DWRITE_FONT_STYLE_NORMAL = 0;
    public const int DWRITE_FONT_STYLE_OBLIQUE = 1;
    public const int DWRITE_FONT_STYLE_ITALIC = 2;

    public const int DWRITE_FONT_STRETCH_UNDEFINED = 0;
    public const int DWRITE_FONT_STRETCH_ULTRA_CONDENSED = 1;
    public const int DWRITE_FONT_STRETCH_EXTRA_CONDENSED = 2;
    public const int DWRITE_FONT_STRETCH_CONDENSED = 3;
    public const int DWRITE_FONT_STRETCH_SEMI_CONDENSED = 4;
    public const int DWRITE_FONT_STRETCH_NORMAL = 5;
    public const int DWRITE_FONT_STRETCH_MEDIUM = 5;
    public const int DWRITE_FONT_STRETCH_SEMI_EXPANDED = 6;
    public const int DWRITE_FONT_STRETCH_EXPANDED = 7;
    public const int DWRITE_FONT_STRETCH_EXTRA_EXPANDED = 8;
    public const int DWRITE_FONT_STRETCH_ULTRA_EXPANDED = 9;

    [LibraryImport(DWRITE)]
    public static partial int DWriteCreateFactory(uint factoryType, in Guid iid, [MarshalUsing(typeof(ComInterfaceMarshaller<IDWriteFactory>))] out IDWriteFactory factory);
}