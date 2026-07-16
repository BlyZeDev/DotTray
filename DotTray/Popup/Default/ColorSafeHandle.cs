namespace DotTray.Popup.Default;

using DotTray.Internal.Native;
using System.Runtime.InteropServices;

/// <summary>
/// Wrapper for a native GDI+ color handle
/// </summary>
/// <remarks>
/// The native handle is destroyed and cleaned up when <see cref="SafeHandle.Dispose()"/> is called
/// </remarks>
public sealed class ColorSafeHandle : SafeHandle
{
    internal ColorSafeHandle(nint handle) : base(nint.Zero, true) => this.handle = handle;

    /// <inheritdoc/>
    public override bool IsInvalid => handle == nint.Zero;

    /// <inheritdoc/>
    protected override bool ReleaseHandle() => PInvoke.GdipDeleteBrush(handle) == 0;
}