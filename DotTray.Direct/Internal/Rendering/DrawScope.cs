namespace DotTray.Direct.Internal.Rendering;

internal readonly ref struct DrawScope : IDisposable
{
    private readonly DirectRenderer _renderer;

    public ID2D1DeviceContext Context { get; }
    public ID2D1SolidColorBrush Brush => _renderer.Brush;

    public DrawScope(DirectRenderer renderer, ID2D1DeviceContext context)
    {
        _renderer = renderer;
        Context = context;
    }

    public void Dispose() => _renderer.EndDraw();
}