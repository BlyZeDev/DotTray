namespace DotTray.Direct.Internal;

using DotTray.Direct.Internal.Models;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("54ec77fa-1377-44e6-8c32-88fd5f44c84c")]
internal partial interface IDXGIDevice;

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("aec22fb8-76f3-4639-9be0-28eb43a67a2e")]
internal partial interface IDXGIObject
{
    void SetPrivateData_Unbound();
    void SetPrivateDataInterface_Unbound();
    void GetPrivateData_Unbound();
    void GetParent_Unbound();
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("3d3e0379-f9de-4d58-bb6c-18d62992f1a6")]
internal partial interface IDXGIDeviceSubObject : IDXGIObject
{
    void GetDevice_Unbound();
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("310d36a0-d2e7-4c0a-aa04-6a9d23b8886a")]
internal partial interface IDXGISwapChain : IDXGIDeviceSubObject
{
    [PreserveSig]
    int Present(uint syncInterval, uint flags);

    void GetBuffer(uint buffer, in Guid riid, out nint surface);

    void SetFullscreenState_Unbound();
    void GetFullscreenState_Unbound();
    void GetDesc_Unbound();
    void ResizeBuffers(uint bufferCount, uint width, uint height, uint newFormat, uint swapChainFlags);
    void ResizeTarget_Unbound();
    void GetContainingOutput_Unbound();
    void GetFrameStatistics_Unbound();
    void GetLastPresentCount_Unbound();
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("790a45f7-0d42-4876-983a-0a55cfe6f4aa")]
internal partial interface IDXGISwapChain1 : IDXGISwapChain;

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("7b7166ec-21c7-44ae-b21a-c9ae321ae369")]
internal partial interface IDXGIFactory : IDXGIObject
{
    void EnumAdapters_Unbound();
    void MakeWindowAssociation_Unbound();
    void GetWindowAssociation_Unbound();
    void CreateSwapChain_Unbound();
    void CreateSoftwareAdapter_Unbound();
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("770aae78-f26f-4dba-a829-253c83d1b387")]
internal partial interface IDXGIFactory1 : IDXGIFactory
{
    void EnumAdapters1_Unbound();
    void IsCurrent_Unbound();
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("50c83a1c-e072-4c48-87b0-3630fa36a6d0")]
internal partial interface IDXGIFactory2 : IDXGIFactory1
{
    void IsWindowedStereoEnabled_Unbound();
    void CreateSwapChainForHwnd_Unbound();
    void CreateSwapChainForCoreWindow_Unbound();
    void GetSharedResourceAdapterLuid_Unbound();
    void RegisterStereoStatusWindow_Unbound();
    void RegisterStereoStatusEvent_Unbound();
    void UnregisterStereoStatus_Unbound();
    void RegisterOcclusionStatusWindow_Unbound();
    void RegisterOcclusionStatusEvent_Unbound();
    void UnregisterOcclusionStatus_Unbound();

    void CreateSwapChainForComposition(IDXGIDevice device, in SwapChainDesc1 desc, nint restrictToOutput, out IDXGISwapChain1 swapChain);
}