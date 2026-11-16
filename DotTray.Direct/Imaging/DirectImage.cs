namespace DotTray.Direct.Imaging;

using DotTray.Direct.Internal;
using DotTray.Direct.Primitives;
using System.Diagnostics.CodeAnalysis;

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

    internal DirectImage(byte[] pixels, int width, int height)
    {
        Pixels = pixels;
        Dimensions = new Dim(width, height);
    }

    /// <summary>
    /// Creates a new image by applying <paramref name="edit"/> to a copy of this one
    /// </summary>
    /// <remarks>
    /// For complex editing use <see cref="ToCanvas"/> to create an actual <see cref="DirectCanvas"/>.<br/>
    /// This <see cref="DirectImage"/> instance is not changed. For several edits in a row, use the chained methods of the canvas inside <paramref name="edit"/>
    /// </remarks>
    /// <param name="edit">The edit to apply</param>
    /// <returns><see cref="DirectImage"/></returns>
    public DirectImage QuickEdit(Action<DirectCanvas> edit)
    {
        ArgumentNullException.ThrowIfNull(edit);

        var canvas = ToCanvas();
        edit(canvas);

        return canvas.ToImage();
    }

    /// <summary>
    /// Creates an editable <see cref="DirectCanvas"/> with a copy of the pixels of this image
    /// </summary>
    /// <remarks>
    /// Edits of the canvas do not change this image
    /// </remarks>
    /// <returns><see cref="DirectCanvas"/></returns>
    public DirectCanvas ToCanvas() => DirectCanvas.FromImage(this);

    /// <summary>
    /// Loads an image from <paramref name="path"/>
    /// </summary>
    /// <remarks>
    /// Supports every format Windows can decode, such as PNG, JPEG, BMP, GIF (first frame), TIFF and ICO
    /// </remarks>
    /// <param name="path">The path to the image file</param>
    /// <returns><see cref="DirectImage"/></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="InvalidDataException"></exception>
    public static DirectImage FromFile(string path)
    {
        var pixels = LoadPixels(path, out var width, out var height);
        return new DirectImage(pixels, width, height);
    }

    /// <summary>
    /// Tries to load an image from <paramref name="path"/>
    /// </summary>
    /// <param name="path">The path to the image file</param>
    /// <param name="image">The image or <see langword="null"/> if it could not be created</param>
    /// <returns><see cref="bool"/></returns>
    public static bool TryFromFile(string path, [NotNullWhen(true)] out DirectImage? image)
    {
        try
        {
            image = FromFile(path);
            return true;
        }
        catch (Exception)
        {
            image = null;
            return false;
        }
    }

    internal static byte[] LoadPixels(string path, out int width, out int height)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("The image file was not found", fullPath);

        return WicImageLoader.Load(fullPath, out width, out height);
    }
}