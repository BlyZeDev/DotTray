namespace DotTray.Internal.Native;

using DotTray.Internal.Win32;
using System;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

internal static partial class PInvoke
{
    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdiplusStartup(out nint token, ref GDIPLUSSTARTUPINPUT input, out nint output);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern void GdiplusShutdown(nint token);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipCreateFromHDC(nint hdc, out nint graphics);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipDeleteGraphics(nint graphics);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipSetSmoothingMode(nint graphics, int smoothingMode);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipCreatePen1(uint color, float width, int unit, out nint pen);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipDeletePen(nint pen);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipDrawLine(nint graphics, nint pen, float x1, float y1, float x2, float y2);

    [DllImport(GdiPlus, SetLastError = true)]
    public static unsafe extern int GdipDrawLines(nint graphics, nint pen, POINTF* points, int count);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipSetPenLineJoin(nint pen, int lineJoin);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipSetPenStartCap(nint pen, int lineCap);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipSetPenEndCap(nint pen, int lineCap);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipCreateSolidFill(uint color, out nint brush);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipDeleteBrush(nint brush);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipFillRectangle(nint graphics, nint brush, float x, float y, float width, float height);

    [DllImport(GdiPlus, CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int GdipMeasureString(nint graphics, string text, int length, nint font, ref RECTF layoutRect, nint stringFormat, out RECTF boundingBox, out int codepointsFitted, out int linesFilled);

    [DllImport(GdiPlus, CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int GdipDrawString(nint graphics, string text, int length, nint font, ref RECTF layoutRect, nint stringFormat, nint brush);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipGetGenericFontFamilySansSerif(out nint fontFamily);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipCreateFont(nint fontFamily, float emSize, int style, int unit, out nint font);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipDeleteFont(nint font);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipDeleteFontFamily(nint fontFamily);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipCreateStringFormat(int formatAttributes, int language, out nint format);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipSetStringFormatFlags(nint format, int flags);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipDeleteStringFormat(nint format);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipSetTextRenderingHint(nint graphics, int mode);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipSetPageUnit(nint graphics, int unit);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipSetPageScale(nint graphics, float scale);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipGetFontHeight(nint font, nint graphics, out float height);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipGetFamily(nint font, out nint fontFamily);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipGetEmHeight(nint fontFamily, int style, out ushort emHeight);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipGetCellAscent(nint fontFamily, int style, out ushort ascent);
}