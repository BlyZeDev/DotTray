namespace DotTray.Direct.Internal.Rendering;

internal readonly struct DrawScope(DirectSurface surface) : IDisposable
{
    public void Dispose() => surface.EndDraw();
}