namespace DotTray.Internal.Native;

using DotTray.Internal.Win32;
using System;
using System.Runtime.InteropServices;

internal static unsafe partial class PInvoke
{
    [DllImport(Gdi32, SetLastError = true)]
    public static extern uint SetTextColor(nint hdc, int crColor);

    [DllImport(Gdi32, SetLastError = true)]
    public static extern int SetBkMode(nint hdc, int mode);

    [DllImport(Gdi32, CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetTextExtentPoint32(nint hdc, string lpString, int c, out SIZE psizl);

    [DllImport(Gdi32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool Polyline(nint hdc, POINT* lppt, int cPoints);

    [DllImport(Gdi32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool RoundRect(nint hdc, int left, int top, int right, int bottom, int width, int height);

    [DllImport(Gdi32, SetLastError = true)]
    public static extern nint CreateSolidBrush(int color);

    [DllImport(Gdi32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool MoveToEx(nint hdc, int X, int Y, nint lpPoint);

    [DllImport(Gdi32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool LineTo(nint hdc, int nXEnd, int nYEnd);

    [DllImport(Gdi32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool Polygon(nint hdc, POINT* pts, int count);

    [DllImport(Gdi32, SetLastError = true)]
    public static extern IntPtr CreatePen(int fnPenStyle, int nWidth, uint crColor);

    [DllImport(Gdi32, CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool TextOut(nint hdc, int x, int y, string lpString, int c);

    [DllImport(Gdi32, SetLastError = true)]
    public static extern nint SelectObject(nint hdc, nint h);

    [DllImport(Gdi32, SetLastError = true)]
    public static extern nint GetStockObject(int fnObject);

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