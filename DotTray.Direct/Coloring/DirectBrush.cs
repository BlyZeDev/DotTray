namespace DotTray.Direct.Coloring;

using DotTray.Direct.Internal;
using DotTray.Direct.Internal.Rendering;
using System.ComponentModel;
using System.Runtime.InteropServices.Marshalling;

/// <summary>
/// Represents a ready to draw Direct brush created by <see cref="IDirectBrush"/>
/// </summary>
public sealed class DirectBrush : IDisposable
{
    private readonly ID2D1Brush _brush;

    private bool disposed;

    internal ID2D1Brush Native
    {
        get
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            return _brush;
        }
    }

    /// <summary>
    /// The native <c>ID2D1Brush*</c>
    /// </summary>
    /// <remarks>
    /// The pointer is borrowed: don't release it, and don't use it after this brush is disposed
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public unsafe nint NativePointer
    {
        get
        {
            ObjectDisposedException.ThrowIf(disposed, this);

            var pointer = ComInterfaceMarshaller<ID2D1Brush>.ConvertToUnmanaged(_brush);
            ComInterfaceMarshaller<ID2D1Brush>.Free(pointer);

            return (nint)pointer;
        }
    }

    internal DirectBrush(ID2D1Brush brush) => _brush = brush;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;

        Com.Release(_brush);
    }
}