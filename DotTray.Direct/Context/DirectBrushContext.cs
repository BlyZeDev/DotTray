using DotTray.Direct.Coloring;

namespace DotTray.Direct.Context;

using DotTray.Direct.Internal.Rendering;

/// <summary>
/// Represents the context passed into <see cref="IDirectBrush.Create(DirectBrushContext, Primitives.Rect)"/>
/// </summary>
public sealed class DirectBrushContext
{
    internal readonly DirectSurface _surface;

    /// <summary>
    /// The native <c>ID2D1DeviceContext*</c> brushes are created on
    /// </summary>
    /// <remarks>
    /// The pointer is borrowed: don't release it and don't use it after the draw call
    /// </remarks>
    public nint NativeContext => _surface.GetContextPointer();

    internal DirectBrushContext(DirectSurface surface) => _surface = surface;
}