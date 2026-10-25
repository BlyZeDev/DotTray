namespace DotTray.Direct.Primitives;

using DotTray.Direct.Context;
using DotTray.Direct.Internal.Rendering;

/// <summary>
/// Restores the previous clip when disposed. Created by <see cref="DrawingContext.Clip(Rect)"/>
/// </summary>
public readonly struct ClipScope : IDisposable
{
    private readonly DirectSurface? _surface;

    internal ClipScope(DirectSurface surface) => _surface = surface;

    /// <inheritdoc/>
    public void Dispose() => _surface?.PopClip();
}