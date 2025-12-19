namespace DotTray.Internal.Native;

using System.Runtime.InteropServices;

internal static unsafe partial class PInvoke
{
    [DllImport(Gdi32, SetLastError = true)]
    public static extern nint SelectObject(nint hdc, nint h);

    [DllImport(Gdi32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeleteObject(nint hObject);

    [DllImport(Gdi32, SetLastError = true)]
    public static extern nint CreateCompatibleDC(nint hdc);

    [DllImport(Gdi32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeleteDC(nint hdc);

    [DllImport(Gdi32, SetLastError = true)]
    public static extern nint CreateCompatibleBitmap(nint hdc, int nWidth, int nHeight);

    [DllImport(Gdi32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool BitBlt(nint hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, nint hdcSrc, int nXSrc, int nYSrc, int dwRop);
}