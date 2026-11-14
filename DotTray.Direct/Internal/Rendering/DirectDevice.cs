namespace DotTray.Direct.Internal.Rendering;

using DotTray.Direct.Coloring;
using DotTray.Direct.Imaging;
using DotTray.Direct.Internal.COM;
using DotTray.Direct.Internal.Models;
using DotTray.Direct.Primitives;
using DotTray.Direct.Typography;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using ResolvedFont = (COM.IDWriteTextFormat Format, Typography.FontMetrics Metrics);

internal sealed class DirectDevice : IDisposable
{
    private const int UserFontCacheCapacity = 32;
    private const int FontCacheCapacity = 128;
    private const int BitmapCacheCapacity = 256;
    private const int MaxBrushLines = 16;

    private readonly ID3D11Device _d3dDevice;
    private readonly ID2D1Device _d2dDevice;
    private readonly ID2D1Factory _d2dFactory;
    private readonly IDXGIDevice _dxgiDevice;
    private readonly IDWriteFactory _dwriteFactory;
    private readonly IDWriteFactory3 _dwriteFactory3;
    private readonly IDCompositionDevice _compositionDevice;
    private readonly IDWriteFontCollection _systemCollection;
    private readonly ID2D1DeviceContext _resourceContext;

    private readonly LruCache<UserFont, IDWriteFontCollection> _userCollections;
    private readonly LruCache<TextInfo, ResolvedFont> _fonts;
    private readonly LruCache<DirectImage, nint> _bitmaps;

    private int referenceCount;

    public bool IsLost { get; private set; }

    public ID2D1StrokeStyle RoundStroke { get; }

    public DirectDevice()
    {
        _d3dDevice = CreateD3DDevice();
        _dxgiDevice = (IDXGIDevice)_d3dDevice;
        _d2dDevice = CreateD2DDevice(_dxgiDevice);

        _d2dFactory = GetFactory(_d2dDevice);
        RoundStroke = CreateRoundStroke(_d2dFactory);

        HR.ThrowIfError(PInvoke.DWriteCreateFactory(0, typeof(IDWriteFactory).GUID, out _dwriteFactory));
        _dwriteFactory3 = (IDWriteFactory3)_dwriteFactory;
        HR.ThrowIfError(_dwriteFactory.GetSystemFontCollection(out _systemCollection, 0));

        HR.ThrowIfError(PInvoke.DCompositionCreateDevice(_dxgiDevice, typeof(IDCompositionDevice).GUID, out _compositionDevice));
        _d2dDevice.CreateDeviceContext(0, out _resourceContext);

        _userCollections = new LruCache<UserFont, IDWriteFontCollection>(UserFontCacheCapacity, onRemoved: static (_, collection) => Com.Release(collection));
        _fonts = new LruCache<TextInfo, ResolvedFont>(FontCacheCapacity, onRemoved: static (_, font) => Com.Release(font.Format));
        _bitmaps = new LruCache<DirectImage, nint>(BitmapCacheCapacity, onRemoved: static (_, bitmap) => Marshal.Release(bitmap));

        referenceCount = 1;
    }

    public DirectDevice Retain()
    {
        int current;

        do
        {
            current = Volatile.Read(ref referenceCount);
            ObjectDisposedException.ThrowIf(current <= 0, this);
        }
        while (Interlocked.CompareExchange(ref referenceCount, current + 1, current) != current);

        return this;
    }

    public bool TryMarkLost(int hr)
    {
        if (!IsDeviceLoss(hr)) return false;

        IsLost = true;
        return true;
    }

    public ID2D1DeviceContext CreateDeviceContext()
    {
        ThrowIfDisposed();

        _d2dDevice.CreateDeviceContext(0, out var context);
        return context;
    }

    public IDXGISwapChain1 CreateSwapChain(uint width, uint height)
    {
        ThrowIfDisposed();

        HR.ThrowIfError(PInvoke.CreateDXGIFactory2(0, typeof(IDXGIFactory2).GUID, out var factory));

        try
        {
            var description = new SwapChainDesc1
            {
                Width = width,
                Height = height,
                Format = PInvoke.DXGI_FORMAT_B8G8R8A8_UNORM,
                SampleDesc = new SampleDesc
                {
                    Count = 1,
                    Quality = 0
                },
                BufferUsage = PInvoke.DXGI_USAGE_RENDER_TARGET_OUTPUT,
                BufferCount = 2,
                Scaling = PInvoke.DXGI_SCALING_STRETCH,
                SwapEffect = PInvoke.DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL,
                AlphaMode = PInvoke.DXGI_ALPHA_MODE_PREMULTIPLIED
            };

            factory.CreateSwapChainForComposition(_dxgiDevice, description, nint.Zero, out var swapChain);
            return swapChain;
        }
        finally
        {
            Com.Release(factory);
        }
    }

    public (IDCompositionTarget Target, IDCompositionVisual Visual) CreateComposition(nint hWnd, IDXGISwapChain1 swapChain)
    {
        ThrowIfDisposed();

        _compositionDevice.CreateTargetForHwnd(hWnd, 1, out var target);
        _compositionDevice.CreateVisual(out var visual);

        visual.SetContent(swapChain);
        target.SetRoot(visual);
        _compositionDevice.Commit();

        return (target, visual);
    }

    public ID2D1Brush CreateSolidBrush(Rgba color)
    {
        ThrowIfDisposed();

        _resourceContext.CreateSolidColorBrush(ToColorF(color), nint.Zero, out var brush);
        return brush;
    }

    public ID2D1Brush CreateLinearGradientBrush(ReadOnlySpan<GradientStop> stops, PointF start, PointF end, GradientExtendMode extendMode)
    {
        ThrowIfDisposed();

        var collection = CreateStopCollection(stops, extendMode);

        try
        {
            var properties = new LinearGradientBrushProperties
            {
                StartPoint = start,
                EndPoint = end
            };

            HR.ThrowIfError(_resourceContext.CreateLinearGradientBrush(properties, nint.Zero, collection, out var brush));
            return brush;
        }
        finally
        {
            Marshal.Release(collection);
        }
    }

    public ID2D1Brush CreateRadialGradientBrush(ReadOnlySpan<GradientStop> stops, PointF center, PointF originOffset, float radiusX, float radiusY, GradientExtendMode extendMode)
    {
        ThrowIfDisposed();

        var collection = CreateStopCollection(stops, extendMode);

        try
        {
            var properties = new RadialGradientBrushProperties
            {
                Center = center,
                GradientOriginOffset = originOffset,
                RadiusX = radiusX,
                RadiusY = radiusY
            };

            HR.ThrowIfError(_resourceContext.CreateRadialGradientBrush(properties, nint.Zero, collection, out var brush));
            return brush;
        }
        finally
        {
            Marshal.Release(collection);
        }
    }

    public IDWriteTextLayout CreateTextLayout(string text, TextInfo info, float maxWidth = float.MaxValue, float maxHeight = float.MaxValue)
    {
        ThrowIfDisposed();

        var format = Resolve(info).Format;

        _dwriteFactory.CreateTextLayout(text, (uint)text.Length, format, maxWidth, maxHeight, out var layout);
        return layout;
    }

    public ID2D1PathGeometry CreatePolyline(ReadOnlySpan<PointF> points)
    {
        ThrowIfDisposed();

        _d2dFactory.CreatePathGeometry(out var geometry);

        try
        {
            geometry.Open(out var sink);

            try
            {
                sink.BeginFigure(points[0], PInvoke.D2D1_FIGURE_BEGIN_HOLLOW);

                for (var i = 1; i < points.Length; i++)
                {
                    sink.AddLine(points[i]);
                }

                sink.EndFigure(PInvoke.D2D1_FIGURE_END_OPEN);
                HR.ThrowIfError(sink.Close());
            }
            finally
            {
                Com.Release(sink);
            }

            return geometry;
        }
        catch
        {
            Com.Release(geometry);
            throw;
        }
    }

    public FontMetrics GetFontMetrics(TextInfo info)
    {
        ThrowIfDisposed();

        return Resolve(info).Metrics;
    }

    public DimF MeasureText(string text, TextInfo info)
    {
        ThrowIfDisposed();

        var layout = CreateTextLayout(text, info);

        try
        {
            layout.GetMetrics(out var metrics);
            return new DimF(metrics.Width, metrics.Height);
        }
        finally
        {
            Com.Release(layout);
        }
    }

    public unsafe RectF GetBrushBounds(IDWriteTextLayout layout, TextInfo info)
    {
        ThrowIfDisposed();

        var capHeight = GetFontMetrics(info).CapHeight;

        layout.GetMetrics(out var metrics);

        var lineBox = new RectF(metrics.Left, metrics.Top, metrics.Width, metrics.Height);
        if (capHeight is not { } cap) return lineBox;

        var lines = stackalloc LineMetrics[MaxBrushLines];
        if (layout.GetLineMetrics(lines, MaxBrushLines, out var count) < 0 || count == 0) return lineBox;

        var first = metrics.Top + lines[0].Baseline - cap;
        var y = metrics.Top;
        var last = first;

        for (var i = 0; i < count; i++)
        {
            last = y + lines[i].Baseline;
            y += lines[i].Height;
        }

        return new RectF(metrics.Left, first, metrics.Width, last - first);
    }

    public unsafe nint GetBitmap(DirectImage image)
    {
        ThrowIfDisposed();

        if (_bitmaps.TryGet(image, out var cached)) return cached;

        var properties = new BitmapProperties
        {
            Format = PInvoke.DXGI_FORMAT_B8G8R8A8_UNORM,
            AlphaMode = PInvoke.D2D1_ALPHA_MODE_PREMULTIPLIED,
            DpiX = 96f,
            DpiY = 96f
        };

        nint bitmap;

        fixed (byte* pixels = image.Pixels)
        {
            _resourceContext.CreateBitmap(new SizeU
            {
                Width = (uint)image.Dimensions.Width,
                Height = (uint)image.Dimensions.Height
            }, (nint)pixels, (uint)image.Dimensions.Width * 4, in properties, out bitmap);
        }

        _bitmaps.Add(image, bitmap);
        return bitmap;
    }

    public void Dispose()
    {
        if (Interlocked.Decrement(ref referenceCount) != 0) return;

        _fonts.Clear();
        _userCollections.Clear();
        _bitmaps.Clear();

        Com.Release(_resourceContext);
        Com.Release(_systemCollection);

        Com.Release(_dwriteFactory);
        Com.Release(_compositionDevice);
        Com.Release(RoundStroke);
        Com.Release(_d2dFactory);
        Com.Release(_d2dDevice);
        Com.Release(_dxgiDevice);
        Com.Release(_d3dDevice);
    }

    private ResolvedFont Resolve(TextInfo info)
    {
        if (_fonts.TryGet(info, out var cached)) return cached;

        var collection = GetCollection(info.Font);

        if (!TryGetFamily(collection, info.Font.Family, out var family)) throw new FontNotFoundException(info.Font.Family);

        try
        {
            _dwriteFactory.CreateTextFormat(info.Font.Family, collection, info.Weight.Value, (int)info.Style, (int)info.Stretch, info.Size, info.Language.Value, out var format);

            try
            {
                format.SetTextAlignment((uint)info.Align);
                format.SetParagraphAlignment(PInvoke.DWRITE_PARAGRAPH_ALIGNMENT_CENTER);
                format.SetWordWrapping(PInvoke.DWRITE_WORD_WRAPPING_NO_WRAP);

                var resolved = new ResolvedFont(format, ReadFontMetrics(family, format, info));
                _fonts.Add(info, resolved);

                return resolved;
            }
            catch
            {
                Com.Release(format);
                throw;
            }
        }
        finally
        {
            Com.Release(family);
        }
    }

    private unsafe FontMetrics ReadFontMetrics(IDWriteFontFamily family, IDWriteTextFormat format, TextInfo info)
    {
        var size = info.Size;

        float? ascent = null, descent = null, lineGap = null, capHeight = null, xHeight = null, stroke = null, underline = null, strikethrough = null;

        if (!HR.IsError(family.GetFirstMatchingFont((uint)info.Weight.Value, (uint)info.Stretch, (uint)info.Style, out var match)))
        {
            try
            {
                match.GetMetrics(out var face);

                if (face.DesignUnitsPerEm != 0)
                {
                    var scale = size / face.DesignUnitsPerEm;

                    ascent = Scaled(face.Ascent);
                    descent = Scaled(face.Descent);
                    lineGap = face.LineGap * scale;
                    capHeight = Scaled(face.CapHeight);
                    xHeight = Scaled(face.XHeight);
                    stroke = Scaled(face.UnderlineThickness);
                    underline = Offset(face.UnderlinePosition);
                    strikethrough = Offset(face.StrikethroughPosition);

                    float? Scaled(float units) => units > 0f ? units * scale : null;
                    float? Offset(float units) => units != 0f ? units * scale : null;
                }
            }
            finally
            {
                Com.Release(match);
            }
        }

        _dwriteFactory.CreateTextLayout("H", 1, format, float.MaxValue, float.MaxValue, out var layout);

        try
        {
            var line = stackalloc LineMetrics[1];
            HR.ThrowIfError(layout.GetLineMetrics(line, 1, out _));

            return new FontMetrics(size, line->Height, line->Baseline, ascent, descent, lineGap, capHeight, xHeight, stroke, underline, strikethrough);
        }
        finally
        {
            Com.Release(layout);
        }
    }

    private IDWriteFontCollection GetCollection(Font font)
    {
        if (font is not UserFont user) return _systemCollection;
        if (_userCollections.TryGet(user, out var cached)) return cached;

        if (!Font.TryCreateCollection(_dwriteFactory3, [.. user.Paths], out var collection))
        {
            throw new InvalidDataException($"Failed to load font '{user.Family}'");
        }

        _userCollections.Add(user, collection);
        return collection;
    }

    private unsafe nint CreateStopCollection(ReadOnlySpan<GradientStop> stops, GradientExtendMode extendMode)
    {
        Span<GradientStopF> native = stops.Length <= 16 ? stackalloc GradientStopF[stops.Length] : new GradientStopF[stops.Length];

        for (var i = 0; i < stops.Length; i++)
        {
            native[i] = new GradientStopF
            {
                Position = stops[i].Offset,
                Color = ToColorF(stops[i].Color)
            };
        }

        fixed (GradientStopF* pointer = native)
        {
            HR.ThrowIfError(_resourceContext.CreateGradientStopCollection(pointer, (uint)native.Length, 0, (uint)extendMode, out var collection));
            return collection;
        }
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(Volatile.Read(ref referenceCount) <= 0, this);

    private static bool IsDeviceLoss(int hr) => (uint)hr is
        PInvoke.DXGI_ERROR_DEVICE_REMOVED or
        PInvoke.DXGI_ERROR_DEVICE_HUNG or
        PInvoke.DXGI_ERROR_DEVICE_RESET or
        PInvoke.DXGI_ERROR_DRIVER_INTERNAL_ERROR or
        PInvoke.D2DERR_RECREATE_TARGET;

    private static bool TryGetFamily(IDWriteFontCollection collection, string name, [NotNullWhen(true)] out IDWriteFontFamily? family)
    {
        family = null;

        return !HR.IsError(collection.FindFamilyName(name, out var index, out var exists))
            && exists != 0
            && !HR.IsError(collection.GetFontFamily(index, out family));
    }

    private static ID3D11Device CreateD3DDevice()
    {
        var hr = TryCreate(PInvoke.D3D_DRIVER_TYPE_HARDWARE, out var device);
        if (HR.IsError(hr)) hr = TryCreate(PInvoke.D3D_DRIVER_TYPE_WARP, out device);

        HR.ThrowIfError(hr);
        return device;

        static int TryCreate(uint driverType, out ID3D11Device device) => PInvoke.D3D11CreateDevice(nint.Zero, driverType, nint.Zero, PInvoke.D3D11_CREATE_DEVICE_BGRA_SUPPORT, nint.Zero, 0, PInvoke.D3D11_SDK_VERSION, out device, nint.Zero, nint.Zero);
    }

    private static ID2D1Device CreateD2DDevice(IDXGIDevice dxgiDevice)
    {
        HR.ThrowIfError(PInvoke.D2D1CreateDevice(dxgiDevice, nint.Zero, out var device));
        return device;
    }

    private static unsafe ID2D1Factory GetFactory(ID2D1Device device)
    {
        device.GetFactory(out var pointer);

        try
        {
            return ComInterfaceMarshaller<ID2D1Factory>.ConvertToManaged((void*)pointer)!;
        }
        finally
        {
            ComInterfaceMarshaller<ID2D1Factory>.Free((void*)pointer);
        }
    }

    private static ID2D1StrokeStyle CreateRoundStroke(ID2D1Factory factory)
    {
        var properties = new StrokeStyleProperties
        {
            StartCap = PInvoke.D2D1_CAP_STYLE_ROUND,
            EndCap = PInvoke.D2D1_CAP_STYLE_ROUND,
            LineJoin = PInvoke.D2D1_LINE_JOIN_ROUND,
            MiterLimit = 1f
        };

        factory.CreateStrokeStyle(properties, nint.Zero, 0, out var stroke);
        return stroke;
    }

    private static ColorF ToColorF(Rgba color) => new ColorF
    {
        R = color.R / (float)byte.MaxValue,
        G = color.G / (float)byte.MaxValue,
        B = color.B / (float)byte.MaxValue,
        A = color.A / (float)byte.MaxValue
    };
}