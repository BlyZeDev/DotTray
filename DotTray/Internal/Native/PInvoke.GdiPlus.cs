namespace DotTray.Internal.Native;

using DotTray.Internal.Win32;
using System.Runtime.InteropServices;

internal static partial class PInvoke
{
    [LibraryImport(GdiPlus, SetLastError = true)]
    public static partial int GdiplusStartup(out nint token, ref GDIPLUSSTARTUPINPUT input, out nint output);

    [LibraryImport(GdiPlus, SetLastError = true)]
    public static partial void GdiplusShutdown(nint token);

    [LibraryImport(GdiPlus, SetLastError = true)]
    public static partial int GdipCreateFromHDC(nint hdc, out nint graphics);

    [LibraryImport(GdiPlus, SetLastError = true)]
    public static partial int GdipDeleteGraphics(nint graphics);

    [LibraryImport(GdiPlus, SetLastError = true)]
    public static partial int GdipSetSmoothingMode(nint graphics, int smoothingMode);

    [LibraryImport(GdiPlus, SetLastError = true)]
    public static partial int GdipCreatePen1(uint color, float width, int unit, out nint pen);

    [LibraryImport(GdiPlus, SetLastError = true)]
    public static partial int GdipDeletePen(nint pen);

    [LibraryImport(GdiPlus, SetLastError = true)]
    public static partial int GdipDrawLine(nint graphics, nint pen, float x1, float y1, float x2, float y2);

    [LibraryImport(GdiPlus, SetLastError = true)]
    public static unsafe partial int GdipDrawLines(nint graphics, nint pen, POINTF* points, int count);

    [LibraryImport(GdiPlus, SetLastError = true)]
    public static partial int GdipCreateSolidFill(uint color, out nint brush);

    [LibraryImport(GdiPlus, SetLastError = true)]
    public static partial int GdipDeleteBrush(nint brush);

    [LibraryImport(GdiPlus, SetLastError = true)]
    public static partial int GdipFillRectangle(nint graphics, nint brush, float x, float y, float width, float height);

    [LibraryImport(GdiPlus, SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial int GdipMeasureString(nint graphics, string text, int length, nint font, ref RECTF layoutRect, nint stringFormat, out RECTF boundingBox, out int codepointsFitted, out int linesFilled);

    [LibraryImport(GdiPlus, SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial int GdipDrawString(nint graphics, string text, int length, nint font, ref RECTF layoutRect, nint stringFormat, nint brush);

    [LibraryImport(GdiPlus, SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial int GdipCreateFontFamilyFromName(string name, nint fontCollection, out nint fontFamily);

    [LibraryImport(GdiPlus, SetLastError = true)]
    public static partial int GdipCreateFont(nint fontFamily, float emSize, int style, int unit, out nint font);

    [LibraryImport(GdiPlus, SetLastError = true)]
    public static partial int GdipDeleteFont(nint font);

    [LibraryImport(GdiPlus, SetLastError = true)]
    public static partial int GdipDeleteFontFamily(nint fontFamily);

    [LibraryImport(GdiPlus, SetLastError = true)]
    public static partial int GdipCreateStringFormat(int formatAttributes, int language, out nint format);

    [LibraryImport(GdiPlus, SetLastError = true)]
    public static partial int GdipSetStringFormatFlags(nint format, int flags);

    [LibraryImport(GdiPlus, SetLastError = true)]
    public static partial int GdipSetStringFormatAlign(nint format, int alignment);

    [LibraryImport(GdiPlus, SetLastError = true)]
    public static partial int GdipSetStringFormatLineAlign(nint format, int alignment);

    [LibraryImport(GdiPlus, SetLastError = true)]
    public static partial int GdipDeleteStringFormat(nint format);

    [LibraryImport(GdiPlus, SetLastError = true)]
    public static unsafe partial int GdipFillPolygon(nint graphics, nint brush, POINTF* points, int count, int fillMode);
}