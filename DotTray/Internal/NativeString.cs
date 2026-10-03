namespace DotTray.Internal;

using System;
using System.Runtime.InteropServices;

internal static class NativeString
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
}