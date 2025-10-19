namespace DotTray.Internal.Native;

using DotTray.Internal.Win32;
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
    public static extern unsafe int GdipDrawLinesI(nint graphics, nint pen, POINT* points, int count);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipSetPenLineJoin(nint pen, int lineJoin);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipSetPenStartCap(nint pen, int lineCap);

    [DllImport(GdiPlus, SetLastError = true)]
    public static extern int GdipSetPenEndCap(nint pen, int lineCap);
}