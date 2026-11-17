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
    void CreateEncoder(in Guid containerFormat, nint vendor, out IWICBitmapEncoder encoder);
    void CreatePalette_Unbound();
    void CreateFormatConverter(out IWICFormatConverter converter);
    void CreateBitmapScaler_Unbound();
    void CreateBitmapClipper_Unbound();
    void CreateBitmapFlipRotator_Unbound();
    void CreateStream(out IWICStream stream);
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

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("0c733a30-2a1c-11ce-ade5-00aa0044773d")]
internal unsafe partial interface ISequentialStream
{
    void Read(byte* buffer, uint count, uint* read);
    void Write(byte* buffer, uint count, uint* written);
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("0000000c-0000-0000-C000-000000000046")]
internal unsafe partial interface IStream : ISequentialStream
{
    void Seek(long move, uint origin, ulong* newPosition);
    void SetSize(ulong size);
    void CopyTo(IStream stream, ulong count, ulong* read, ulong* written);
    void Commit(uint flags);
    void Revert();
    void LockRegion(ulong offset, ulong count, uint lockType);
    void UnlockRegion(ulong offset, ulong count, uint lockType);
    void Stat(nint stat, uint flags);
    void Clone(out IStream stream);
}

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16, Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("135ff860-22b7-4ddf-b0f6-218f4f299a43")]
internal unsafe partial interface IWICStream : IStream
{
    void InitializeFromIStream_Unbound();
    void InitializeFromFilename(string fileName, uint desiredAccess);
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("00000103-a8f2-4877-ba0a-fd2b6645fb94")]
internal partial interface IWICBitmapEncoder
{
    void Initialize(IStream stream, uint cacheOption);
    void GetContainerFormat_Unbound();
    void GetEncoderInfo_Unbound();
    void SetColorContexts_Unbound();
    void SetPalette_Unbound();
    void SetThumbnail_Unbound();
    void SetPreview_Unbound();
    void CreateNewFrame(out IWICBitmapFrameEncode frame, nint encoderOptions);
    void Commit();
    void GetMetadataQueryWriter_Unbound();
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("00000105-a8f2-4877-ba0a-fd2b6645fb94")]
internal unsafe partial interface IWICBitmapFrameEncode
{
    void Initialize(nint encoderOptions);
    void SetSize(uint width, uint height);
    void SetResolution_Unbound();
    void SetPixelFormat(ref Guid pixelFormat);
    void SetColorContexts_Unbound();
    void SetPalette_Unbound();
    void SetThumbnail_Unbound();
    void WritePixels(uint lineCount, uint stride, uint bufferSize, byte* pixels);
    void WriteSource_Unbound();
    void Commit();
}