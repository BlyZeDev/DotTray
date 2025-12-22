namespace DotTray.Internal.Native;

using System.Runtime.InteropServices;

internal static unsafe partial class PInvoke
{
    [LibraryImport(Gdi32, SetLastError = true)]
    public static partial nint SelectObject(nint hdc, nint h);

    [LibraryImport(Gdi32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DeleteObject(nint hObject);

    [LibraryImport(Gdi32, SetLastError = true)]
    public static partial nint CreateCompatibleDC(nint hdc);

    [LibraryImport(Gdi32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DeleteDC(nint hdc);

    [LibraryImport(Gdi32, SetLastError = true)]
    public static partial nint CreateCompatibleBitmap(nint hdc, int nWidth, int nHeight);

    [LibraryImport(Gdi32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool BitBlt(nint hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, nint hdcSrc, int nXSrc, int nYSrc, int dwRop);
}