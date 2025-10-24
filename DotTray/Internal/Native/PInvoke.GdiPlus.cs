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
    public static extern int GdipDrawString(nint graphics, string text, int length, nint font, ref RECT layoutRect, nint stringFormat, nint brush);
}