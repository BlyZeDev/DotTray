namespace DotTray.Direct.Internal;

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

internal static partial class PInvoke
{
    private const string DWRITE = "dwrite.dll";

    public const uint DWRITE_MEASURING_MODE_NATURAL = 0;
    public const int DWRITE_FONT_WEIGHT_NORMAL = 400;
    public const int DWRITE_FONT_WEIGHT_SEMI_BOLD = 600;
    public const int DWRITE_FONT_STYLE_NORMAL = 0;
    public const int DWRITE_FONT_STRETCH_NORMAL = 5;
    public const uint DWRITE_TEXT_ALIGNMENT_LEADING = 0;
    public const uint DWRITE_TEXT_ALIGNMENT_TRAILING = 1;
    public const uint DWRITE_TEXT_ALIGNMENT_CENTER = 2;
    public const uint DWRITE_PARAGRAPH_ALIGNMENT_NEAR = 0;
    public const uint DWRITE_PARAGRAPH_ALIGNMENT_CENTER = 2;
    public const uint DWRITE_WORD_WRAPPING_WRAP = 0;
    public const uint DWRITE_WORD_WRAPPING_NO_WRAP = 1;

    [LibraryImport(DWRITE)]
    public static partial int DWriteCreateFactory(uint factoryType, in Guid iid, [MarshalUsing(typeof(ComInterfaceMarshaller<IDWriteFactory>))] out IDWriteFactory factory);
}