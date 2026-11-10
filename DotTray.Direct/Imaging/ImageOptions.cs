namespace DotTray.Direct.Imaging;

using DotTray.Direct.Primitives;

/// <summary>
/// Represents the options used to draw a <see cref="DirectImage"/>
/// </summary>
public sealed record ImageOptions
{
    /// <summary>
    /// The default options: fully opaque, linear interpolation
    /// </summary>
    public static ImageOptions Default { get; } = new ImageOptions();

    /// <summary>
    /// The opacity from <c>0f</c> (invisible) to <c>1f</c> (opaque)
    /// </summary>
    public float Opacity
    {
        get;
        init => field = Math.Clamp(value, 0f, 1f);
    } = 1f;

    /// <summary>
    /// The interpolation mode used when the image is scaled
    /// </summary>
    public InterpolationMode InterpolationMode { get; init; } = InterpolationMode.Linear;

    /// <summary>
    /// The part of the image to draw, in image pixels, or <see langword="null"/> to draw the whole image
    /// </summary>
    /// <remarks>
    /// Should lie inside the image. The part is stretched to the destination bounds, so this can be used to crop
    /// </remarks>
    public Rect? SourceBounds { get; init; }
}