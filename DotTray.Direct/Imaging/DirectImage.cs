namespace DotTray.Direct.Imaging;

using DotTray.Direct.Internal;
using DotTray.Direct.Primitives;

/// <summary>
/// Represents a decoded image that can be drawn with <see cref="Context.DrawingContext.DrawImage(DirectImage, RectF, ImageOptions)"/>
/// </summary>
/// <remarks>
/// The image is decoded once and can be shared by any number of items and popups
/// </remarks>
public sealed class DirectImage
{
    internal byte[] Pixels { get; }

    /// <summary>
    /// The dimensions of the image, in pixels
    /// </summary>
    public Dim Dimensions { get; }

    private DirectImage(byte[] pixels, int width, int height)
    {
        Pixels = pixels;
        Dimensions = new Dim(width, height);
    }

    /// <summary>
    /// Loads an image from <paramref name="path"/>
    /// </summary>
    /// <remarks>
    /// Supports every format Windows can decode, such as PNG, JPEG, BMP, GIF (first frame), TIFF and ICO
    /// </remarks>
    /// <param name="path">The path to the image file</param>
    /// <returns><see cref="DirectImage"/></returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="InvalidDataException"></exception>
    public static DirectImage FromFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("The image file was not found", fullPath);

        var pixels = WicImageLoader.Load(fullPath, out var width, out var height);
        return new DirectImage(pixels, width, height);
    }
}