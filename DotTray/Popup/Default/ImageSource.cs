namespace DotTray.Popup.Default;

using DotTray.Internal.Native;
using DotTray.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using CacheImage = (ImageSource Image, int Width, int Height);

/// <summary>
/// Represents an GDI+ image source
/// </summary>
public sealed class ImageSource : IDisposable
{
    private const int MaxCachedScales = 4;

    private readonly LinkedList<CacheImage> _scaledCache;

    private bool disposed;
    internal nint Handle { get; }

    /// <summary>
    /// The size of the image in pixels
    /// </summary>
    public Size Size { get; }

    private ImageSource(nint handle, Size size)
    {
        _scaledCache = new LinkedList<CacheImage>();

        Handle = handle;
        Size = size;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (disposed) return;

        foreach (var (image, _, _) in _scaledCache) image.Dispose();
        _scaledCache.Clear();

        PInvoke.GdipDisposeImage(Handle);
        disposed = true;
        GC.SuppressFinalize(this);
    }

    internal ImageSource GetScaled(int width, int height)
    {
        if (width <= 0 || height <= 0) return this;
        if (width == Size.Width && height == Size.Height) return this;

        for (var node = _scaledCache.First; node is not null; node = node.Next)
        {
            if (node.Value.Width == width && node.Value.Height == height)
            {
                _scaledCache.Remove(node);
                _scaledCache.AddFirst(node);
                return node.Value.Image;
            }
        }

        var scaled = new ImageSource(RenderScaled(Handle, width, height), new Size(width, height));
        _scaledCache.AddFirst((scaled, width, height));
        
        if (_scaledCache.Count > MaxCachedScales)
        {
            var evicted = _scaledCache.Last!.Value.Image;
            _scaledCache.RemoveLast();
            evicted.Dispose();
        }

        return scaled;
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

    /// <summary>
    /// Wraps an existing HIcon as a <see cref="ImageSource"/>
    /// </summary>
    /// <param name="hIcon">The handle to a GDI bitmap</param>
    /// <returns><see cref="ImageSource"/></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static ImageSource FromHIcon(nint hIcon)
    {
        var status = PInvoke.GdipCreateBitmapFromHICON(hIcon, out var hImage);
        if (status is not (int)PInvoke.GdiPlusStatus.Ok) throw new InvalidOperationException("Failed to create image from HICON");

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

        var materialized = RenderScaled(hImage, (int)width, (int)height);
        PInvoke.GdipDisposeImage(hImage);

        return new ImageSource(materialized, new Size((int)width, (int)height));
    }

    private static nint RenderScaled(nint source, int width, int height)
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
            PInvoke.GdipSetInterpolationMode(hGraphics, PInvoke.InterpolationModeHighQuality);
            PInvoke.GdipSetSmoothingMode(hGraphics, PInvoke.SmoothingModeAntiAlias8x8);
            PInvoke.GdipSetPixelOffsetMode(hGraphics, PInvoke.PixelOffsetModeHalf);
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