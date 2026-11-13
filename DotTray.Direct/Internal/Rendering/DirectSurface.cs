namespace DotTray.Direct.Internal.Rendering;

using DotTray.Direct.Coloring;
using DotTray.Direct.Context;
using DotTray.Direct.Imaging;
using DotTray.Direct.Internal.COM;
using DotTray.Direct.Internal.Models;
using DotTray.Direct.Primitives;
using DotTray.Direct.Typography;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

internal sealed class DirectSurface : IDisposable
{
    private const float BaseDpi = 96f;

    private static readonly Guid IID_IDXGISurface = new Guid("cafcb56c-6ac3-4889-bf47-9e23bbd260ec");
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

    private uint curPixWidth;
    private uint curPixHeight;
    private float curDpi;
    private bool disposed;

    public float Scale => curDpi / BaseDpi;

    public bool IsLost => _device.IsLost;

    public DirectSurface(DirectDevice device, nint hWnd, int pixelWidth, int pixelHeight, float dpi)
    {
        _device = device;
        curPixWidth = ToPixels(pixelWidth);
        curPixHeight = ToPixels(pixelHeight);
        curDpi = dpi;

        _context = _device.CreateDeviceContext();
        _context.SetTextAntialiasMode(PInvoke.D2D1_TEXT_ANTIALIAS_MODE_GRAYSCALE);

        _swapChain = _device.CreateSwapChain(curPixWidth, curPixHeight);
        (_compositionTarget, _compositionVisual) = _device.CreateComposition(hWnd, _swapChain);

        CreateTarget();
    }

    public unsafe nint GetContextPointer()
    {
        ObjectDisposedException.ThrowIf(disposed, this);

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
        ObjectDisposedException.ThrowIf(disposed, this);

        var hr = _context.EndDraw(nint.Zero, nint.Zero);
        if (hr >= 0) hr = _swapChain.Present(1, 0);

        if (_device.TryMarkLost(hr)) return;

        HR.ThrowIfError(hr);
    }

    public void Resize(int pixelWidth, int pixelHeight, float dpi)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        var width = ToPixels(pixelWidth);
        var height = ToPixels(pixelHeight);
        if (width == curPixWidth && height == curPixHeight && dpi == curDpi) return;

        curPixWidth = width;
        curPixHeight = height;
        curDpi = dpi;

        try
        {
            _context.SetTarget(nint.Zero);
            _swapChain.ResizeBuffers(0, width, height, 0, 0);

            CreateTarget();
        }
        catch (COMException ex) when (_device.TryMarkLost(ex.HResult)) { }
    }

    public ID2D1Brush CreateSolidBrush(Rgba color)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return _device.CreateSolidBrush(color);
    }

    public ID2D1Brush CreateLinearGradientBrush(ReadOnlySpan<GradientStop> stops, PointF start, PointF end, GradientExtendMode extendMode)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return _device.CreateLinearGradientBrush(stops, start, end, extendMode);
    }

    public ID2D1Brush CreateRadialGradientBrush(ReadOnlySpan<GradientStop> stops, PointF center, PointF originOffset, float radiusX, float radiusY, GradientExtendMode extendMode)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return _device.CreateRadialGradientBrush(stops, center, originOffset, radiusX, radiusY, extendMode);
    }

    public void FillRect(RectF rect, ID2D1Brush brush)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        _context.FillRectangle(Snap(rect), brush);
    }

    public void DrawRect(RectF rect, ID2D1Brush brush, float strokeWidth)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        _context.DrawRectangle(Snap(rect), brush, strokeWidth, nint.Zero);
    }

    public void FillRoundedRect(RectF rect, float radius, ID2D1Brush brush)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        _context.FillRoundedRectangle(new RoundedRectF
        {
            Rect = Snap(rect),
            RadiusX = radius,
            RadiusY = radius
        }, brush);
    }

    public void DrawRoundedRect(RectF rect, float radius, ID2D1Brush brush, float strokeWidth)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        _context.DrawRoundedRectangle(new RoundedRectF
        {
            Rect = Snap(rect),
            RadiusX = radius,
            RadiusY = radius
        }, brush, strokeWidth, nint.Zero);
    }

    public void FillEllipse(RectF rect, ID2D1Brush brush)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        _context.FillEllipse(new EllipseF
        {
            Point = new PointF
            {
                X = rect.X + rect.Width / 2f,
                Y = rect.Y + rect.Height / 2f
            },
            RadiusX = rect.Width / 2f,
            RadiusY = rect.Height / 2f
        }, brush);
    }

    public void DrawEllipse(RectF rect, ID2D1Brush brush, float strokeWidth)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        _context.DrawEllipse(new EllipseF
        {
            Point = new PointF
            {
                X = rect.X + rect.Width / 2f,
                Y = rect.Y + rect.Height / 2f
            },
            RadiusX = rect.Width / 2f,
            RadiusY = rect.Height / 2f
        }, brush, strokeWidth, nint.Zero);
    }

    public void DrawLine(PosF start, PosF end, float strokeWidth, ID2D1Brush brush)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        _context.DrawLine(new PointF
        {
            X = start.X,
            Y = start.Y
        }, new PointF
        {
            X = end.X,
            Y = end.Y
        }, brush, strokeWidth, nint.Zero);
    }

    public void DrawPolyline(ReadOnlySpan<PointF> points, float strokeWidth, ID2D1Brush brush)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        var geometry = _device.CreatePolyline(points);

        try
        {
            _context.DrawGeometry(geometry, brush, strokeWidth, _device.RoundStroke);
        }
        finally
        {
            Com.Release(geometry);
        }
    }

    public unsafe void DrawImage(DirectImage image, RectF destination, ImageOptions options)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        var bitmap = _device.GetBitmap(image);
        var target = Snap(destination);

        var pSource = nint.Zero;
        if (options.SourceBounds.HasValue)
        {
            var source = new RECTF
            {
                Left = options.SourceBounds.Value.Left,
                Top = options.SourceBounds.Value.Top,
                Right = options.SourceBounds.Value.Right,
                Bottom = options.SourceBounds.Value.Bottom
            };
            pSource = (nint)(&source);
        }

        _context.DrawBitmapDc(bitmap, target, options.Opacity, (uint)options.InterpolationMode, pSource, nint.Zero);
    }

    public FontMetrics GetFontMetrics(TextInfo info)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return _device.GetFontMetrics(info);
    }

    public DimF MeasureText(string text, TextInfo info)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return _device.MeasureText(text, info);
    }

    public void DrawText(string text, TextInfo info, RectF bounds, IDirectBrush brush, DirectBrushContext context)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        if (text.Length == 0) return;

        var layout = _device.CreateTextLayout(text, info, bounds.Width, bounds.Height);

        try
        {
            var brushBounds = _device.GetBrushBounds(layout, info);
            var textBounds = new RectF(bounds.X + brushBounds.X, bounds.Y + brushBounds.Y, brushBounds.Width, brushBounds.Height);

            using var native = brush.Create(context, textBounds);

            _context.DrawTextLayout(new PointF
            {
                X = bounds.X,
                Y = bounds.Y
            }, layout, native.Native, PInvoke.D2D1_DRAW_TEXT_OPTIONS_CLIP | PInvoke.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT);
        }
        finally
        {
            Com.Release(layout);
        }
    }

    public void PushClip(Rect rect)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        _context.PushAxisAlignedClip(Snap(rect), PInvoke.D2D1_ANTIALIAS_MODE_ALIASED);
    }

    public void PopClip()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        _context.PopAxisAlignedClip();
    }

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
                DpiX = curDpi,
                DpiY = curDpi,
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

        _context.SetDpi(curDpi, curDpi);
    }

    private RECTF Snap(RectF rect) => new()
    {
        Left = SnapEdge(rect.Left),
        Top = SnapEdge(rect.Top),
        Right = SnapEdge(rect.Right),
        Bottom = SnapEdge(rect.Bottom)
    };

    private float SnapEdge(float dips) => MathF.Round(dips * Scale) / Scale;

    private static uint ToPixels(int value) => (uint)int.Max(1, value);
}