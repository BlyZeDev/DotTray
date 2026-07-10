namespace DotTray.Internal;

using DotTray.Internal.Win32;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

internal static class NativeUtil
{
    public static unsafe void WriteFixed(char* destination, int capacity, string? value)
    {
        if (capacity <= 0) return;

        if (string.IsNullOrEmpty(value))
        {
            destination[0] = char.MinValue;
            return;
        }

        var destinationSpan = MemoryMarshal.CreateSpan(ref destination[0], capacity);
        var source = value.AsSpan();

        var length = Math.Min(source.Length, capacity - 1);

        source[..length].CopyTo(destinationSpan);
        destination[length] = char.MinValue;
    }

    public static uint ToGdip(this TrayColor c) => (uint)(c.A << 24 | c.R << 16 | c.G << 8 | c.B);

    public static POINT ToPOINT(this Point point)
    {
        return new POINT
        {
            x = point.X,
            y = point.Y,
        };
    }

    public static POINTF ToPOINTF(this Point point)
    {
        return new POINTF
        {
            X = point.X,
            Y = point.Y,
        };
    }

    public static RECTF ToRECTF(this SizeF size, float x, float y)
    {
        return new RECTF
        {
            X = x,
            Y = y,
            Width = size.Width,
            Height = size.Height
        };
    }

    public static RECTF ToRECTF(this RectangleF rect)
    {
        return new RECTF
        {
            X = rect.X,
            Y = rect.Y,
            Width = rect.Width,
            Height = rect.Height
        };
    }
}