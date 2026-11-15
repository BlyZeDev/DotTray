namespace DotTray.Direct.Internal.Rendering;

internal readonly struct DrawScope : IDisposable
{
    private readonly DirectSurface _surface;

    public DrawScope(DirectSurface surface) => _surface = surface;

    public void Dispose() => _surface.EndDraw();
}