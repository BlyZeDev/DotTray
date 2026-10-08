namespace DotTray.Direct.Internal.Rendering;

using DotTray.Direct.Internal.Models;
using System.Globalization;
using System.Runtime.InteropServices;

internal sealed class DirectRenderer : IDisposable
{
    private const float MaxLayoutSize = 10000f;

    private static readonly Guid IID_IDXGISurface = new("cafcb56c-6ac3-4889-bf47-9e23bbd260ec");
    private static readonly Matrix3x2 Identity = new Matrix3x2
    {
        M11 = 1,
        M22 = 1
    };

    private readonly ID3D11Device _d3dDevice;
    private readonly IDXGIDevice _dxgiDevice;
    private readonly ID2D1Device _d2dDevice;
    private readonly ID2D1DeviceContext _context;
    private readonly ID2D1SolidColorBrush _brush;
    private readonly IDWriteFactory _dwriteFactory;
    private readonly IDXGISwapChain1 _swapChain;
    private readonly IDCompositionDevice _compositionDevice;
    private readonly IDCompositionTarget _compositionTarget;
    private readonly IDCompositionVisual _compositionVisual;

    private uint pixelWidth;
    private uint pixelHeight;
    private bool disposed;

    public float Dpi { get; private set; }

    public float Scale => Dpi / 96f;

    public (float Width, float Height) SizeDips => (pixelWidth / Scale, pixelHeight / Scale);

    public bool IsDeviceLost { get; private set; }

    public ID2D1SolidColorBrush Brush => _brush;

    public DirectRenderer(nint hWnd, int width, int height, float dpi)
    {
        this.pixelWidth = ToPixels(width);
        this.pixelHeight = ToPixels(height);
        Dpi = dpi;

        _d3dDevice = CreateD3DDevice();
        _dxgiDevice = (IDXGIDevice)_d3dDevice;
        _d2dDevice = CreateD2DDevice(_dxgiDevice);

        _d2dDevice.CreateDeviceContext(0, out _context);
        _context.SetTextAntialiasMode(PInvoke.D2D1_TEXT_ANTIALIAS_MODE_GRAYSCALE);
        _context.CreateSolidColorBrush(new ColorF { R = 1, G = 1, B = 1, A = 1 }, nint.Zero, out _brush);

        HR.ThrowIfError(PInvoke.DWriteCreateFactory(0, typeof(IDWriteFactory).GUID, out _dwriteFactory));

        _swapChain = CreateSwapChain(_dxgiDevice, this.pixelWidth, this.pixelHeight);
        (_compositionDevice, _compositionTarget, _compositionVisual) = CreateComposition(_dxgiDevice, hWnd, _swapChain);

        CreateTarget();
    }

    public DrawScope BeginDraw()
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        _context.BeginDraw();
        _context.SetTransform(Identity);

        return new DrawScope(this, _context);
    }

    public void Resize(int pixelWidth, int pixelHeight, float dpi)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        this.pixelWidth = ToPixels(pixelWidth);
        this.pixelHeight = ToPixels(pixelHeight);
        Dpi = dpi;

        _context.SetTarget(nint.Zero);
        _swapChain.ResizeBuffers(0, this.pixelWidth, this.pixelHeight, 0, 0);

        CreateTarget();
    }

    public IDWriteTextFormat CreateTextFormat(string family = "Segoe UI", float size = 14f, int weight = PInvoke.DWRITE_FONT_WEIGHT_NORMAL)
    {
        var locale = CultureInfo.CurrentUICulture.Name is { Length: > 0 } name ? name : "en-us";

        _dwriteFactory.CreateTextFormat(family, nint.Zero, weight, PInvoke.DWRITE_FONT_STYLE_NORMAL, PInvoke.DWRITE_FONT_STRETCH_NORMAL, size, locale, out var format);
        return format;
    }

    public IDWriteTextLayout CreateTextLayout(string text, IDWriteTextFormat format, float maxWidth = MaxLayoutSize, float maxHeight = MaxLayoutSize)
    {
        _dwriteFactory.CreateTextLayout(text, (uint)text.Length, format, maxWidth, maxHeight, out var layout);
        return layout;
    }

    public (float Width, float Height) MeasureText(string text, IDWriteTextFormat format, float maxWidth = MaxLayoutSize)
    {
        var layout = CreateTextLayout(text, format, maxWidth);

        try
        {
            layout.GetMetrics(out var metrics);
            return (metrics.Width, metrics.Height);
        }
        finally
        {
            Release(layout);
        }
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;

        _context.SetTarget(nint.Zero);

        Release(_dwriteFactory);
        Release(_brush);
        Release(_compositionVisual);
        Release(_compositionTarget);
        Release(_compositionDevice);
        Release(_swapChain);
        Release(_context);
        Release(_d2dDevice);
        Release(_dxgiDevice);
        Release(_d3dDevice);
    }

    public bool EndDraw()
    {
        var hr = _context.EndDraw(nint.Zero, nint.Zero);
        if (hr >= 0) hr = _swapChain.Present(1, 0);

        if (HR.IsDeviceLost(hr))
        {
            IsDeviceLost = true;
            return false;
        }

        HR.ThrowIfError(hr);
        return true;
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
                DpiX = Dpi,
                DpiY = Dpi,
                Options = PInvoke.D2D1_BITMAP_OPTIONS_TARGET | PInvoke.D2D1_BITMAP_OPTIONS_CANNOT_DRAW
            };

            _context.CreateBitmapFromDxgiSurface(surface, in properties, out bitmap);
            _context.SetTarget(bitmap);
        }
        finally
        {
            if (bitmap != nint.Zero) Marshal.Release(bitmap);
            Marshal.Release(surface);
        }

        _context.SetDpi(Dpi, Dpi);
    }

    private static ID3D11Device CreateD3DDevice()
    {
        var hr = TryCreate(PInvoke.D3D_DRIVER_TYPE_HARDWARE, out var device);
        if (hr < 0) hr = TryCreate(PInvoke.D3D_DRIVER_TYPE_WARP, out device);

        HR.ThrowIfError(hr);
        return device;

        static int TryCreate(uint driverType, out ID3D11Device device)
            => PInvoke.D3D11CreateDevice(nint.Zero, driverType, nint.Zero, PInvoke.D3D11_CREATE_DEVICE_BGRA_SUPPORT, nint.Zero, 0, PInvoke.D3D11_SDK_VERSION, out device, nint.Zero, nint.Zero);
    }

    private static ID2D1Device CreateD2DDevice(IDXGIDevice dxgiDevice)
    {
        HR.ThrowIfError(PInvoke.D2D1CreateDevice(dxgiDevice, nint.Zero, out var device));
        return device;
    }

    private static IDXGISwapChain1 CreateSwapChain(IDXGIDevice dxgiDevice, uint width, uint height)
    {
        HR.ThrowIfError(PInvoke.CreateDXGIFactory2(0, typeof(IDXGIFactory2).GUID, out var factory));

        try
        {
            var description = new SwapChainDesc1
            {
                Width = width,
                Height = height,
                Format = PInvoke.DXGI_FORMAT_B8G8R8A8_UNORM,
                SampleDesc = new SampleDesc { Count = 1, Quality = 0 },
                BufferUsage = PInvoke.DXGI_USAGE_RENDER_TARGET_OUTPUT,
                BufferCount = 2,
                Scaling = PInvoke.DXGI_SCALING_STRETCH,
                SwapEffect = PInvoke.DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL,
                AlphaMode = PInvoke.DXGI_ALPHA_MODE_PREMULTIPLIED
            };

            factory.CreateSwapChainForComposition(dxgiDevice, in description, nint.Zero, out var swapChain);
            return swapChain;
        }
        finally
        {
            Release(factory);
        }
    }

    private static (IDCompositionDevice Device, IDCompositionTarget Target, IDCompositionVisual Visual) CreateComposition(IDXGIDevice dxgiDevice, nint hWnd, IDXGISwapChain1 swapChain)
    {
        HR.ThrowIfError(PInvoke.DCompositionCreateDevice(dxgiDevice, typeof(IDCompositionDevice).GUID, out var device));

        device.CreateTargetForHwnd(hWnd, 1, out var target);
        device.CreateVisual(out var visual);

        visual.SetContent(swapChain);
        target.SetRoot(visual);
        device.Commit();

        return (device, target, visual);
    }

    private static uint ToPixels(int value) => (uint)int.Max(1, value);

    private static void Release(object? comObject)
    {
        if (comObject is null) return;

        if (comObject is IDisposable disposable) disposable.Dispose();
        else if (Marshal.IsComObject(comObject)) Marshal.FinalReleaseComObject(comObject);
    }
}