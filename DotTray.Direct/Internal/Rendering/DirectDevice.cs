namespace DotTray.Direct.Internal.Rendering;

using DotTray.Direct.Imaging;
using DotTray.Direct.Internal.COM;
using DotTray.Direct.Internal.Models;
using DotTray.Direct.Primitives;
using DotTray.Direct.Typography;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

internal sealed class DirectDevice : IDisposable
{
    private const float FallbackCapHeightRatio = 0.7f;

    private readonly ID3D11Device _d3dDevice;
    private readonly IDXGIDevice _dxgiDevice;
    private readonly IDWriteFactory _dwriteFactory;
    private readonly IDWriteFactory3 _dwriteFactory3;
    private readonly IDCompositionDevice _compositionDevice;
    private readonly IDWriteFontCollection _systemCollection;
    private readonly ID2D1DeviceContext _resourceContext;
    private readonly Dictionary<UserFont, IDWriteFontCollection> _userCollections = [];
    private readonly Dictionary<TextInfo, ResolvedFont> _fonts = [];
    private readonly Dictionary<DirectImage, nint> _bitmaps = [];

    private bool disposed;

    public ID2D1Device D2DDevice { get; }
    public ID2D1Factory D2DFactory { get; }

    public ID2D1StrokeStyle RoundStroke { get; }

    private sealed record ResolvedFont(IDWriteTextFormat Format, float CapHeight);

    public DirectDevice()
    {
        _d3dDevice = CreateD3DDevice();
        _dxgiDevice = (IDXGIDevice)_d3dDevice;
        D2DDevice = CreateD2DDevice(_dxgiDevice);

        D2DFactory = GetFactory(D2DDevice);
        RoundStroke = CreateRoundStroke(D2DFactory);

        HR.ThrowIfError(PInvoke.DWriteCreateFactory(0, typeof(IDWriteFactory).GUID, out _dwriteFactory));
        _dwriteFactory3 = (IDWriteFactory3)_dwriteFactory;
        HR.ThrowIfError(_dwriteFactory.GetSystemFontCollection(out _systemCollection, 0));

        HR.ThrowIfError(PInvoke.DCompositionCreateDevice(_dxgiDevice, typeof(IDCompositionDevice).GUID, out _compositionDevice));
        D2DDevice.CreateDeviceContext(0, out _resourceContext);
    }

    public IDXGISwapChain1 CreateSwapChain(uint width, uint height)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

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
        ObjectDisposedException.ThrowIf(disposed, this);

        _compositionDevice.CreateTargetForHwnd(hWnd, 1, out var target);
        _compositionDevice.CreateVisual(out var visual);

        visual.SetContent(swapChain);
        target.SetRoot(visual);
        _compositionDevice.Commit();

        return (target, visual);
    }

    public IDWriteTextLayout CreateTextLayout(string text, TextInfo info, float maxWidth = float.MaxValue, float maxHeight = float.MaxValue)
    {
        var format = Resolve(info).Format;

        _dwriteFactory.CreateTextLayout(text, (uint)text.Length, format, maxWidth, maxHeight, out var layout);
        return layout;
    }

    public DimF MeasureText(string text, TextInfo info)
    {
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

    public float GetCapHeight(TextInfo info) => Resolve(info).CapHeight;

    public ID2D1PathGeometry CreatePolyline(ReadOnlySpan<PointF> points)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        D2DFactory.CreatePathGeometry(out var geometry);

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

    public unsafe nint GetBitmap(DirectImage image)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        if (_bitmaps.TryGetValue(image, out var cached)) return cached;

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

        _bitmaps[image] = bitmap;
        return bitmap;
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;

        foreach (var resolved in _fonts.Values)
        {
            Com.Release(resolved.Format);
        }
        _fonts.Clear();

        foreach (var collection in _userCollections.Values)
        {
            Com.Release(collection);
        }
        _userCollections.Clear();

        foreach (var bitmap in _bitmaps.Values)
        {
            Marshal.Release(bitmap);
        }
        _bitmaps.Clear();

        Com.Release(_resourceContext);
        Com.Release(_systemCollection);

        Com.Release(_dwriteFactory);
        Com.Release(_compositionDevice);
        Com.Release(RoundStroke);
        Com.Release(D2DFactory);
        Com.Release(D2DDevice);
        Com.Release(_dxgiDevice);
        Com.Release(_d3dDevice);
    }

    private ResolvedFont Resolve(TextInfo info)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        if (_fonts.TryGetValue(info, out var cached)) return cached;

        var collection = GetCollection(info.Font);

        if (!TryGetFamily(collection, info.Font.Family, out var family)) throw new FontNotFoundException(info.Font.Family);

        try
        {
            var capHeight = ReadCapHeight(family, info);

            _dwriteFactory.CreateTextFormat(info.Font.Family, collection, info.Weight.Value, (int)info.Style, (int)info.Stretch, info.Size, info.Language.Value, out var format);
            format.SetTextAlignment((uint)info.Align);
            format.SetParagraphAlignment(PInvoke.DWRITE_PARAGRAPH_ALIGNMENT_CENTER);
            format.SetWordWrapping(PInvoke.DWRITE_WORD_WRAPPING_NO_WRAP);

            var resolved = new ResolvedFont(format, capHeight);
            _fonts[info] = resolved;

            return resolved;
        }
        finally
        {
            Com.Release(family);
        }
    }

    private IDWriteFontCollection GetCollection(Font font)
    {
        if (font is not UserFont user) return _systemCollection;
        if (_userCollections.TryGetValue(user, out var cached)) return cached;

        if (!Font.TryCreateCollection(_dwriteFactory3, [.. user.Paths], out var collection))
        {
            throw new InvalidDataException($"Failed to load font '{user.Family}'");
        }

        _userCollections[user] = collection;
        return collection;
    }

    private static bool TryGetFamily(IDWriteFontCollection collection, string name, out IDWriteFontFamily family)
    {
        family = null!;

        return !HR.IsError(collection.FindFamilyName(name, out var index, out var exists))
            && exists != 0
            && !HR.IsError(collection.GetFontFamily(index, out family));
    }

    private static float ReadCapHeight(IDWriteFontFamily family, TextInfo info)
    {
        var fallback = info.Size * FallbackCapHeightRatio;

        if (HR.IsError(family.GetFirstMatchingFont((uint)info.Weight.Value, (uint)info.Stretch, (uint)info.Style, out var match))) return fallback;

        try
        {
            match.GetMetrics(out var metrics);
            if (metrics.DesignUnitsPerEm == 0) return fallback;

            var units = metrics.CapHeight != 0 ? metrics.CapHeight : metrics.Ascent;
            return units * info.Size / metrics.DesignUnitsPerEm;
        }
        finally
        {
            Com.Release(match);
        }
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
}