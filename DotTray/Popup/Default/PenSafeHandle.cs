namespace DotTray.Popup.Default;

using DotTray.Internal.Native;
using System.Runtime.InteropServices;

/// <summary>
/// Wrapper for a native GDI+ pen handle
/// </summary>
/// <remarks>
/// The native handle is destroyed and cleaned up when <see cref="SafeHandle.Dispose()"/> is called
/// </remarks>
public sealed class PenSafeHandle : SafeHandle
{
    /// <inheritdoc/>
    public override bool IsInvalid => handle == nint.Zero;

    internal PenSafeHandle(nint handle) : base(nint.Zero, true) => this.handle = handle;

    /// <inheritdoc/>
    protected override bool ReleaseHandle() => PInvoke.GdipDeletePen(handle) == 0;
}