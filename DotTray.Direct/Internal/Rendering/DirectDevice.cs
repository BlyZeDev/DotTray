namespace DotTray.Direct.Internal.Rendering;

using DotTray.Direct.Internal.Models;
using DotTray.Direct.Primitives;
using DotTray.Direct.Typography;
using System.Globalization;

internal sealed class DirectDevice : IDisposable
{
    private readonly ID3D11Device _d3dDevice;
    private readonly IDXGIDevice _dxgiDevice;
    private readonly IDWriteFactory _dwriteFactory;
    private readonly IDCompositionDevice _compositionDevice;
    private readonly Dictionary<FontInfo, IDWriteTextFormat> _formatCache = [];

    private bool disposed;

    public ID2D1Device D2DDevice { get; }

    public DirectDevice()
    {
        _d3dDevice = CreateD3DDevice();
        _dxgiDevice = (IDXGIDevice)_d3dDevice;
        D2DDevice = CreateD2DDevice(_dxgiDevice);

        HR.ThrowIfError(PInvoke.DWriteCreateFactory(0, typeof(IDWriteFactory).GUID, out _dwriteFactory));
        HR.ThrowIfError(PInvoke.DCompositionCreateDevice(_dxgiDevice, typeof(IDCompositionDevice).GUID, out _compositionDevice));
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

    public IDWriteTextLayout CreateTextLayout(string text, FontInfo font, float maxWidth = float.MaxValue, float maxHeight = float.MaxValue)
    {
        var format = GetTextFormat(font);

        _dwriteFactory.CreateTextLayout(text, (uint)text.Length, format, maxWidth, maxHeight, out var layout);
        return layout;
    }

    public DimF MeasureText(string text, FontInfo font)
    {
        var layout = CreateTextLayout(text, font);

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

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;

        foreach (var format in _formatCache.Values)
        {
            Com.Release(format);
        }
        _formatCache.Clear();

        Com.Release(_dwriteFactory);
        Com.Release(_compositionDevice);
        Com.Release(D2DDevice);
        Com.Release(_dxgiDevice);
        Com.Release(_d3dDevice);
    }

    private IDWriteTextFormat GetTextFormat(FontInfo font)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        if (_formatCache.TryGetValue(font, out var cached)) return cached;

        var locale = CultureInfo.CurrentUICulture.Name is { Length: > 0 } name ? name : "en-US"; // Add Language Tag in FontInfo instead of hardcoding lol
        
        _dwriteFactory.CreateTextFormat(font.Family, nint.Zero, font.Weight.Value, (int)font.Style, (int)font.Stretch, font.Size, locale, out var format);

        format.SetTextAlignment((uint)font.Align);
        format.SetParagraphAlignment(PInvoke.DWRITE_PARAGRAPH_ALIGNMENT_CENTER);
        format.SetWordWrapping(PInvoke.DWRITE_WORD_WRAPPING_NO_WRAP);

        _formatCache[font] = format;
        return format;
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
}