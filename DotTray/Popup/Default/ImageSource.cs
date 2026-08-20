namespace DotTray.Popup.Default;

using DotTray.Internal.Native;
using DotTray.Primitives;
using System;
using System.IO;

/// <summary>
/// Represents an GDI+ image source
/// </summary>
public sealed class ImageSource : IDisposable
{
    private bool disposed;
    internal nint Handle { get; }

    /// <summary>
    /// The size of the image in pixels
    /// </summary>
    public Size Size { get; }

    private ImageSource(nint handle, Size size)
    {
        Handle = handle;
        Size = size;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (disposed) return;

        PInvoke.GdipDisposeImage(Handle);
        disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Loads an image from a file path
    /// </summary>
    /// <param name="path">The path to the image</param>
    /// <returns><see cref="ImageSource"/></returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static ImageSource FromFile(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("The file does not exist", path);

        var status = PInvoke.GdipLoadImageFromFile(path, out var hImage);
        if (status is not (int)PInvoke.GdiPlusStatus.Ok) throw new InvalidOperationException($"Failed to load image from '{path}'");

        return FromHandle(hImage);
    }

    /// <summary>
    /// Wraps an existing HBITMAP as a <see cref="ImageSource"/>
    /// </summary>
    /// <param name="hBitmap">The handle to a GDI bitmap</param>
    /// <returns><see cref="ImageSource"/></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static ImageSource FromHBitmap(nint hBitmap)
    {
        var status = PInvoke.GdipCreateBitmapFromHBITMAP(hBitmap, nint.Zero, out var hImage);
        if (status is not (int)PInvoke.GdiPlusStatus.Ok) throw new InvalidOperationException("Failed to created image from HBitmap");

        return FromHandle(hImage);
    }

    private static ImageSource FromHandle(nint hImage)
    {
        var status = PInvoke.GdipGetImageWidth(hImage, out var width);
        if (status is not (int)PInvoke.GdiPlusStatus.Ok)
        {
            PInvoke.GdipDisposeImage(hImage);
            throw new InvalidOperationException("Failed to read the image dimensions");
        }

        status = PInvoke.GdipGetImageHeight(hImage, out var height);
        if (status is not (int)PInvoke.GdiPlusStatus.Ok)
        {
            PInvoke.GdipDisposeImage(hImage);
            throw new InvalidOperationException("Failed to read the image dimensions");
        }

        var materialized = Materialize(hImage, (int)width, (int)height);
        PInvoke.GdipDisposeImage(hImage);

        return new ImageSource(materialized, new Size((int)width, (int)height));
    }

    private static nint Materialize(nint source, int width, int height)
    {
        var status = PInvoke.GdipCreateBitmapFromScan0(width, height, 0, PInvoke.PixelFormat32bppPARGB, nint.Zero, out var hDest);
        if (status is not (int)PInvoke.GdiPlusStatus.Ok)
        {
            throw new InvalidOperationException("Failed to materialize bitmap");
        }

        status = PInvoke.GdipGetImageGraphicsContext(hDest, out var hGraphics);
        if (status is not (int)PInvoke.GdiPlusStatus.Ok)
        {
            PInvoke.GdipDisposeImage(hDest);
            throw new InvalidOperationException("Failed to get graphics context");
        }

        try
        {
            PInvoke.GdipDrawImageRectI(hGraphics, source, 0, 0, width, height);
        }
        finally
        {
            PInvoke.GdipDeleteGraphics(hGraphics);
        }

        return hDest;
    }

    /// <summary>
    /// Finalizes this instance
    /// </summary>
    ~ImageSource() => PInvoke.GdipDisposeImage(Handle);
}