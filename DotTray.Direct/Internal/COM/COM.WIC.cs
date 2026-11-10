namespace DotTray.Direct.Internal.COM;

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16, Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("ec5ec8a9-c395-4314-9c77-54d7a935ff70")]
internal partial interface IWICImagingFactory
{
    void CreateDecoderFromFilename(string fileName, nint vendor, uint desiredAccess, uint metadataOptions, out IWICBitmapDecoder decoder);
    void CreateDecoderFromStream_Unbound();
    void CreateDecoderFromFileHandle_Unbound();
    void CreateComponentInfo_Unbound();
    void CreateDecoder_Unbound();
    void CreateEncoder_Unbound();
    void CreatePalette_Unbound();
    void CreateFormatConverter(out IWICFormatConverter converter);
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("9edde9e7-8dee-47ea-99df-e6faf2ed44bf")]
internal partial interface IWICBitmapDecoder
{
    void QueryCapability_Unbound();
    void Initialize_Unbound();
    void GetContainerFormat_Unbound();
    void GetDecoderInfo_Unbound();
    void CopyPalette_Unbound();
    void GetMetadataQueryReader_Unbound();
    void GetPreview_Unbound();
    void GetColorContexts_Unbound();
    void GetThumbnail_Unbound();
    void GetFrameCount_Unbound();
    void GetFrame(uint index, out IWICBitmapFrameDecode frame);
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("00000120-a8f2-4877-ba0a-fd2b6645fb94")]
internal unsafe partial interface IWICBitmapSource
{
    void GetSize(out uint width, out uint height);
    void GetPixelFormat(out Guid pixelFormat);
    void GetResolution_Unbound();
    void CopyPalette_Unbound();
    void CopyPixels(nint rect, uint stride, uint bufferSize, byte* buffer);
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("3b16811b-6a43-4ec9-a813-3d930c13b940")]
internal unsafe partial interface IWICBitmapFrameDecode : IWICBitmapSource;

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("00000301-a8f2-4877-ba0a-fd2b6645fb94")]
internal unsafe partial interface IWICFormatConverter : IWICBitmapSource
{
    void Initialize(IWICBitmapSource source, in Guid destinationFormat, uint dither, nint palette, double alphaThresholdPercent, uint paletteTranslate);
}