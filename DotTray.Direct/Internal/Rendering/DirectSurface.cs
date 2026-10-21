namespace DotTray.Direct.Internal.Rendering;

using DotTray.Direct;
using DotTray.Direct.Coloring;
using DotTray.Direct.Internal.Models;
using DotTray.Direct.Primitives;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

internal sealed class DirectSurface : IDisposable
{
    private const float BaseDpi = 96f;

    private static readonly Guid IID_IDXGISurface = new("cafcb56c-6ac3-4889-bf47-9e23bbd260ec");
    private static readonly Matrix3x2 Identity = new Matrix3x2
    {
        M11 = 1,
        M22 = 1
    };

    private readonly DirectDevice _device;
    private readonly ID2D1DeviceContext _context;
    private readonly IDXGISwapChain1 _swapChain;
    private readonly IDCompositionTarget _compositionTarget;
    private readonly IDCompositionVisual _compositionVisual;

    private uint width;
    private uint height;
    private float dpi;
    private bool disposed;

    public float Scale => dpi / BaseDpi;

    public DirectSurface(DirectDevice device, nint hWnd, int width, int height, float dpi)
    {
        _device = device;
        this.width = ToPixels(width);
        this.height = ToPixels(height);
        this.dpi = dpi;

        _device.D2DDevice.CreateDeviceContext(0, out _context);
        _context.SetTextAntialiasMode(PInvoke.D2D1_TEXT_ANTIALIAS_MODE_GRAYSCALE);

        _swapChain = _device.CreateSwapChain(this.width, this.height);
        (_compositionTarget, _compositionVisual) = _device.CreateComposition(hWnd, _swapChain);

        CreateTarget();
    }

    public unsafe nint GetContextPointer()
    {
        var pointer = ComInterfaceMarshaller<ID2D1DeviceContext>.ConvertToUnmanaged(_context);
        ComInterfaceMarshaller<ID2D1DeviceContext>.Free(pointer);

        return (nint)pointer;
    }

    public DrawScope BeginDraw()
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        _context.BeginDraw();
        _context.SetTransform(Identity);
        _context.Clear(new ColorF());

        return new DrawScope(this);
    }

    public void EndDraw()
    {
        var hr = _context.EndDraw(nint.Zero, nint.Zero);
        if (hr >= 0) hr = _swapChain.Present(1, 0);

        HR.ThrowIfError(hr);
    }

    public void Resize(int pixelWidth, int pixelHeight, float dpi)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        var width = ToPixels(pixelWidth);
        var height = ToPixels(pixelHeight);
        if (width == this.width && height == this.height && dpi == this.dpi) return;

        this.width = width;
        this.height = height;
        this.dpi = dpi;

        _context.SetTarget(nint.Zero);
        _swapChain.ResizeBuffers(0, width, height, 0, 0);

        CreateTarget();
    }

    public ID2D1Brush CreateSolidBrush(Rgba color)
    {
        _context.CreateSolidColorBrush(ToColorF(color), nint.Zero, out var brush);
        return brush;
    }

    public void FillRect(Rect rect, ID2D1Brush brush) => _context.FillRectangle(Snap(rect), brush);

    public void FillRoundedRect(Rect rect, float radius, ID2D1Brush brush)
    {
        _context.FillRoundedRectangle(new RoundedRectF
        { 
            Rect = Snap(rect),
            RadiusX = radius,
            RadiusY = radius
        }
        , brush);
    }

    public void DrawLine(float x1, float y1, float x2, float y2, float width, ID2D1Brush brush)
    {
        _context.DrawLine(new PointF
        {
            X = x1,
            Y = y1
        }, new PointF
        {
            X = x2,
            Y = y2
        }, brush, width, nint.Zero);
    }

    public void DrawText(string text, FontInfo font, Rect bounds, ID2D1Brush brush)
    {
        if (text.Length == 0) return;

        var layout = _device.CreateTextLayout(text, font, bounds.Width, bounds.Height);

        try
        {
            _context.DrawTextLayout(new PointF
            {
                X = bounds.X,
                Y = bounds.Y
            }, layout, brush, PInvoke.D2D1_DRAW_TEXT_OPTIONS_CLIP | PInvoke.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT);
        }
        finally
        {
            Com.Release(layout);
        }
    }

    public void PushClip(Rect rect) => _context.PushAxisAlignedClip(Snap(rect), PInvoke.D2D1_ANTIALIAS_MODE_ALIASED);

    public void PopClip() => _context.PopAxisAlignedClip();

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;

        _context.SetTarget(nint.Zero);

        Com.Release(_compositionVisual);
        Com.Release(_compositionTarget);
        Com.Release(_swapChain);
        Com.Release(_context);
    }

    private void CreateTarget()
    {
        _swapChain.GetBuffer(0, IID_IDXGISurface, out var surface);
        var bitmap = nint.Zero;

        try
        {
            var properties = new BitmapProperties1
            {
                Format = PInvoke.DXGI_FORMAT_B8G8R8A8_UNORM,
                AlphaMode = PInvoke.D2D1_ALPHA_MODE_PREMULTIPLIED,
                DpiX = dpi,
                DpiY = dpi,
                Options = PInvoke.D2D1_BITMAP_OPTIONS_TARGET | PInvoke.D2D1_BITMAP_OPTIONS_CANNOT_DRAW
            };

            _context.CreateBitmapFromDxgiSurface(surface, properties, out bitmap);
            _context.SetTarget(bitmap);
        }
        finally
        {
            if (bitmap != nint.Zero) Marshal.Release(bitmap);
            Marshal.Release(surface);
        }

        _context.SetDpi(dpi, dpi);
    }

    private RECTF Snap(Rect rect) => new()
    {
        Left = SnapEdge(rect.Left),
        Top = SnapEdge(rect.Top),
        Right = SnapEdge(rect.Right),
        Bottom = SnapEdge(rect.Bottom)
    };

    private float SnapEdge(float dips) => MathF.Round(dips * Scale) / Scale;

    private static uint ToPixels(int value) => (uint)int.Max(1, value);

    private static ColorF ToColorF(Rgba color) => new ColorF
    {
        R = color.R / (float)byte.MaxValue,
        G = color.G / (float)byte.MaxValue,
        B = color.B / (float)byte.MaxValue,
        A = color.A / (float)byte.MaxValue
    };
}