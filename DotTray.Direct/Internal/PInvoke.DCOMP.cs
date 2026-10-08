namespace DotTray.Direct.Internal;

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

internal static partial class PInvoke
{
    private const string DCOMP = "dcomp.dll";

    [LibraryImport(DCOMP)]
    public static partial int DCompositionCreateDevice([MarshalUsing(typeof(ComInterfaceMarshaller<IDXGIDevice>))] IDXGIDevice dxgiDevice, in Guid iid, [MarshalUsing(typeof(ComInterfaceMarshaller<IDCompositionDevice>))] out IDCompositionDevice device);
}