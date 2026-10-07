namespace DotTray.Direct.Internal;

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

internal static partial class PInvoke
{
    private const string D3D11 = "d3d11.dll";

    public const uint D3D_DRIVER_TYPE_HARDWARE = 1;
    public const uint D3D_DRIVER_TYPE_WARP = 5;
    public const uint D3D11_CREATE_DEVICE_BGRA_SUPPORT = 0x20;
    public const uint D3D11_SDK_VERSION = 7;

    [LibraryImport(D3D11)]
    public static partial int D3D11CreateDevice(nint adapter, uint driverType, nint software, uint flags, nint featureLevels, uint featureLevelCount, uint sdkVersion, [MarshalUsing(typeof(ComInterfaceMarshaller<COM.ID3D11Device>))] out COM.ID3D11Device device, nint featureLevel, nint immediateContext);
}