namespace DotTray.Primitives;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a location and size
/// </summary>
public readonly record struct RectangleF
{
    /// <summary>
    /// The X-coordinate of the upper-left corner of this <see cref="RectangleF"/> instance
    /// </summary>
    public required readonly float X { get; init; }
    /// <summary>
    /// The Y-coordinate of the upper-left corner of this <see cref="RectangleF"/> instance
    /// </summary>
    public required readonly float Y { get; init; }
    /// <summary>
    /// The Width of this <see cref="RectangleF"/> instance
    /// </summary>
    public required readonly float Width { get; init; }
    /// <summary>
    /// The Height of this <see cref="RectangleF"/> instance
    /// </summary>
    public required readonly float Height { get; init; }

    /// <summary>
    /// <inheritdoc cref="X"/>
    /// </summary>
    public readonly float Left => X;
    /// <summary>
    /// <inheritdoc cref="Y"/>
    /// </summary>
    public readonly float Top => Y;
    /// <summary>
    /// The X-coordinate of the lower-right corner of this <see cref="RectangleF"/> instance
    /// </summary>
    public readonly float Right => unchecked(X + Width);
    /// <summary>
    /// The Y-coordinate of the lower-right corner of this <see cref="RectangleF"/> instance
    /// </summary>
    public readonly float Bottom => unchecked(Y + Height);

    /// <summary>
    /// Initializes a new instance of <see cref="RectangleF"/>
    /// </summary>
    /// <param name="x">The X-coordinate of the upper-left corner</param>
    /// <param name="y">The Y-coordinate of the upper-left corner</param>
    /// <param name="width">The width</param>
    /// <param name="height">The height</param>
    [SetsRequiredMembers]
    public RectangleF(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Implicitly converts <see cref="Rectangle"/> to <see cref="RectangleF"/>
    /// </summary>
    /// <param name="rect">The rectangle to convert</param>
    public static implicit operator RectangleF(Rectangle rect) => new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
}