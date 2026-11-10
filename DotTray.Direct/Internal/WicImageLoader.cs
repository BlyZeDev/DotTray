namespace DotTray.Direct.Internal;

using DotTray.Direct.Internal.COM;
using DotTray.Direct.Internal.Rendering;
using System.Runtime.InteropServices.Marshalling;

internal static unsafe partial class WicImageLoader
{
    private const uint ClsctxInprocServer = 0x1;
    private const uint CoinitMultithreaded = 0x0;
    private const int RpcEChangedMode = unchecked((int)0x80010106);
    private const uint GenericRead = 0x80000000;
    private const uint MetadataCacheOnDemand = 0;
    private const uint DitherTypeNone = 0;
    private const uint PaletteTypeCustom = 0;

    private static readonly Guid ClsidImagingFactory = new Guid("cacaf262-9370-4615-a13b-9f5539da4c0a");
    private static readonly Guid IidImagingFactory = new Guid("ec5ec8a9-c395-4314-9c77-54d7a935ff70");
    private static readonly Guid PixelFormat32bppPBGRA = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc910");

    public static byte[] Load(string path, out int width, out int height)
    {
        var init = PInvoke.CoInitializeEx(nint.Zero, CoinitMultithreaded);
        if (init < 0 && init != RpcEChangedMode) HR.ThrowIfError(init);

        var uninitialize = init >= 0;

        IWICImagingFactory? factory = null;
        IWICBitmapDecoder? decoder = null;
        IWICBitmapFrameDecode? frame = null;
        IWICFormatConverter? converter = null;

        try
        {
            HR.ThrowIfError(PInvoke.CoCreateInstance(ClsidImagingFactory, nint.Zero, ClsctxInprocServer, IidImagingFactory, out var pointer));

            try
            {
                factory = ComInterfaceMarshaller<IWICImagingFactory>.ConvertToManaged((void*)pointer)!;
            }
            finally
            {
                ComInterfaceMarshaller<IWICImagingFactory>.Free((void*)pointer);
            }

            factory.CreateDecoderFromFilename(path, nint.Zero, GenericRead, MetadataCacheOnDemand, out decoder);
            decoder.GetFrame(0, out frame);

            IWICBitmapSource source = frame;

            frame.GetPixelFormat(out var format);

            if (format != PixelFormat32bppPBGRA)
            {
                factory.CreateFormatConverter(out converter);
                converter.Initialize(frame, PixelFormat32bppPBGRA, DitherTypeNone, nint.Zero, 0.0, PaletteTypeCustom);

                source = converter;
            }

            source.GetSize(out var w, out var h);
            if (w == 0 || h == 0) throw new InvalidDataException($"The image '{path}' is empty");

            var size = (long)w * h * 4;
            if (size > int.MaxValue) throw new InvalidDataException($"The image '{path}' is too large");

            var pixels = GC.AllocateUninitializedArray<byte>((int)size);

            fixed (byte* buffer = pixels)
            {
                source.CopyPixels(nint.Zero, w * 4, (uint)size, buffer);
            }

            width = (int)w;
            height = (int)h;
            return pixels;
        }
        finally
        {
            if (converter is not null) Com.Release(converter);
            if (frame is not null) Com.Release(frame);
            if (decoder is not null) Com.Release(decoder);
            if (factory is not null) Com.Release(factory);

            if (uninitialize) PInvoke.CoUninitialize();
        }
    }
}