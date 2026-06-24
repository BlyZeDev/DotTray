namespace DotTray;

using DotTray.Internal.Native;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

public sealed class NotifyIconException : Exception
{
    private NotifyIconException(string? message, Exception? innerException) : base(message, innerException) { }

    internal static void ThrowIfFalse(bool result, string message)
    {
        if (result) return;

        ThrowWin32(message);
    }

    internal static void ThrowIfZero<TNumber>(TNumber value, string message) where TNumber : struct, INumber<TNumber>
    {
        if (value != TNumber.Zero) return;

        ThrowWin32(message);
    }

    internal static void ThrowIfNull(nint status, string message)
    {
        if (status != nint.Zero) return;

        ThrowWin32(message);
    }

    internal static void ThrowIfNotOk(PInvoke.GdiPlusStatus status, string message)
    {
        if (status is PInvoke.GdiPlusStatus.Ok) return;

        ThrowGdiPlus(status, message);
    }

    [DoesNotReturn]
    private static void ThrowWin32(string message)
    {
        var exception = new NotifyIconException(message, new Win32Exception(Marshal.GetLastPInvokeError(), Marshal.GetLastPInvokeErrorMessage()));
        throw exception;
    }

    [DoesNotReturn]
    private static void ThrowGdiPlus(PInvoke.GdiPlusStatus status, string message)
    {
        var exception = new NotifyIconException(message, new Win32Exception((int)status, $"GDI+ Error: {status}"));
        throw exception;
    }
}