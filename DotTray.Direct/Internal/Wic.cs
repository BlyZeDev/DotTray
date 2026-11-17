namespace DotTray.Direct.Internal;

using DotTray.Direct.Coloring;
using DotTray.Direct.Internal.COM;
using DotTray.Direct.Internal.Rendering;
using System.Runtime.InteropServices.Marshalling;

internal static unsafe class Wic
{
    private const uint ClsctxInprocServer = 0x1;
    private const uint CoinitMultithreaded = 0x0;
    private const int RpcEChangedMode = unchecked((int)0x80010106);
    private const uint GenericRead = 0x80000000;
    private const uint GenericWrite = 0x40000000;
    private const uint EncoderCacheInMemory = 0;
    private const uint MetadataCacheOnDemand = 0;
    private const uint DitherTypeNone = 0;
    private const uint PaletteTypeCustom = 0;

    private static readonly Guid ClsidImagingFactory = new Guid("cacaf262-9370-4615-a13b-9f5539da4c0a");
    private static readonly Guid IidImagingFactory = new Guid("ec5ec8a9-c395-4314-9c77-54d7a935ff70");
    private static readonly Guid PixelFormat32bppPBGRA = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc910");
    private static readonly Guid PixelFormat32bppBGRA = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc90f");
    private static readonly Guid PixelFormat24bppBGR = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc90c");

    private static readonly Guid ContainerFormatPng = new Guid("1b7cfaf4-713f-473c-bbcd-6137425faeaf");
    private static readonly Guid ContainerFormatJpeg = new Guid("19e4a5aa-5662-4fc5-a0c0-1758028e1057");
    private static readonly Guid ContainerFormatBmp = new Guid("0af1d87e-fcfe-4188-bdeb-a7906471cbe3");

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

    public static void Save(string path, byte[] pixels, int width, int height, Rgba? background)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var fullPath = Path.GetFullPath(path);

        var (container, hasAlpha) = Path.GetExtension(fullPath).ToLowerInvariant() switch
        {
            ".png" => (ContainerFormatPng, true),
            ".jpg" or ".jpeg" => (ContainerFormatJpeg, false),
            ".bmp" => (ContainerFormatBmp, false),
            var extension => throw new ArgumentException($"The file extension '{extension}' is not supported, use .png, .jpg, .jpeg or .bmp", nameof(path))
        };

        var data = hasAlpha ? ToStraightBgra(pixels) : ToOpaqueBgr(pixels, background ?? Rgba.White);
        var format = hasAlpha ? PixelFormat32bppBGRA : PixelFormat24bppBGR;
        var stride = width * (hasAlpha ? 4 : 3);

        var init = PInvoke.CoInitializeEx(nint.Zero, CoinitMultithreaded);
        if (init < 0 && init != RpcEChangedMode) HR.ThrowIfError(init);

        var uninitialize = init >= 0;

        IWICImagingFactory? factory = null;
        IWICStream? stream = null;
        IWICBitmapEncoder? encoder = null;
        IWICBitmapFrameEncode? frame = null;

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

            factory.CreateStream(out stream);
            stream.InitializeFromFilename(fullPath, GenericWrite);

            factory.CreateEncoder(in container, nint.Zero, out encoder);
            encoder.Initialize(stream, EncoderCacheInMemory);
            encoder.CreateNewFrame(out frame, nint.Zero);

            frame.Initialize(nint.Zero);
            frame.SetSize((uint)width, (uint)height);

            var negotiated = format;
            frame.SetPixelFormat(ref negotiated);
            if (negotiated != format) throw new InvalidDataException("The encoder does not support the required pixel format");

            fixed (byte* buffer = data)
            {
                frame.WritePixels((uint)height, (uint)stride, (uint)data.Length, buffer);
            }

            frame.Commit();
            encoder.Commit();
        }
        catch
        {
            Com.Release(frame);
            Com.Release(encoder);
            Com.Release(stream);
            frame = null;
            encoder = null;
            stream = null;

            try
            {
                File.Delete(fullPath);
            }
            catch (Exception) { }

            throw;
        }
        finally
        {
            Com.Release(frame);
            Com.Release(encoder);
            Com.Release(stream);
            Com.Release(factory);

            if (uninitialize) PInvoke.CoUninitialize();
        }
    }

    private static byte[] ToStraightBgra(byte[] premultiplied)
    {
        var result = new byte[premultiplied.Length];

        for (var i = 0; i < premultiplied.Length; i += 4)
        {
            var a = premultiplied[i + 3];
            if (a == 0) continue;

            result[i + 3] = a;

            if (a == 255)
            {
                result[i] = premultiplied[i];
                result[i + 1] = premultiplied[i + 1];
                result[i + 2] = premultiplied[i + 2];
                continue;
            }

            result[i] = (byte)Math.Min(255, (premultiplied[i] * 255 + a / 2) / a);
            result[i + 1] = (byte)Math.Min(255, (premultiplied[i + 1] * 255 + a / 2) / a);
            result[i + 2] = (byte)Math.Min(255, (premultiplied[i + 2] * 255 + a / 2) / a);
        }

        return result;
    }

    private static byte[] ToOpaqueBgr(byte[] premultiplied, Rgba background)
    {
        var count = premultiplied.Length / 4;
        var result = new byte[count * 3];

        for (var i = 0; i < count; i++)
        {
            var inverse = 255 - premultiplied[i * 4 + 3];

            result[i * 3] = (byte)Math.Min(255, premultiplied[i * 4] + (background.B * inverse + 127) / 255);
            result[i * 3 + 1] = (byte)Math.Min(255, premultiplied[i * 4 + 1] + (background.G * inverse + 127) / 255);
            result[i * 3 + 2] = (byte)Math.Min(255, premultiplied[i * 4 + 2] + (background.R * inverse + 127) / 255);
        }

        return result;
    }
}