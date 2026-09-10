namespace DotTray.Popup.Default;

using DotTray.Internal.Native;
using System.Runtime.InteropServices;

/// <summary>
/// Wrapper for a native GDI+ brush handle
/// </summary>
/// <remarks>
/// The native handle is destroyed and cleaned up when <see cref="SafeHandle.Dispose()"/> is called
/// </remarks>
public sealed class BrushSafeHandle : SafeHandle
{
    /// <inheritdoc/>
    public override bool IsInvalid => handle == nint.Zero;

    internal BrushSafeHandle(nint handle) : base(nint.Zero, true) => this.handle = handle;

    /// <inheritdoc/>
    protected override bool ReleaseHandle() => PInvoke.GdipDeleteBrush(handle) == 0;
}