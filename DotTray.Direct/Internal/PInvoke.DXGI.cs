namespace DotTray.Direct.Internal;

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

internal static partial class PInvoke
{
    private const string DXGI = "dxgi.dll";

    public const uint DXGI_FORMAT_B8G8R8A8_UNORM = 87;
    public const uint DXGI_USAGE_RENDER_TARGET_OUTPUT = 0x20;
    public const uint DXGI_SCALING_STRETCH = 0;
    public const uint DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL = 3;
    public const uint DXGI_ALPHA_MODE_PREMULTIPLIED = 1;

    [LibraryImport(DXGI)]
    public static partial int CreateDXGIFactory2(uint flags, in Guid riid, [MarshalUsing(typeof(ComInterfaceMarshaller<COM.IDXGIFactory2>))] out COM.IDXGIFactory2 factory);
}