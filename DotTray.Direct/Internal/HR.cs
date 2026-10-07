namespace DotTray.Direct.Internal;

using System.Runtime.InteropServices;

internal static class HR
{
    private const int D2DERR_RECREATE_TARGET = unchecked((int)0x8899000C);
    private const int DXGI_ERROR_DEVICE_REMOVED = unchecked((int)0x887A0005);
    private const int DXGI_ERROR_DEVICE_RESET = unchecked((int)0x887A0007);

    public static void Check(int hr)
    {
        if (hr < 0) Marshal.ThrowExceptionForHR(hr);
    }

    public static bool IsDeviceLost(int hr) => hr is D2DERR_RECREATE_TARGET or DXGI_ERROR_DEVICE_RESET or DXGI_ERROR_DEVICE_REMOVED;
}