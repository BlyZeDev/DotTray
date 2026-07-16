namespace DotTray.Primitives;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a location and size
/// </summary>
public readonly record struct Rectangle
{
    /// <summary>
    /// The X-coordinate of the upper-left corner of this <see cref="Rectangle"/> instance
    /// </summary>
    public required readonly int X { get; init; }
    /// <summary>
    /// The Y-coordinate of the upper-left corner of this <see cref="Rectangle"/> instance
    /// </summary>
    public required readonly int Y { get; init; }
    /// <summary>
    /// The Width of this <see cref="Rectangle"/> instance
    /// </summary>
    public required readonly int Width { get; init; }
    /// <summary>
    /// The Height of this <see cref="Rectangle"/> instance
    /// </summary>
    public required readonly int Height { get; init; }

    /// <summary>
    /// <inheritdoc cref="X"/>
    /// </summary>
    public readonly int Left => X;
    /// <summary>
    /// <inheritdoc cref="Y"/>
    /// </summary>
    public readonly int Top => Y;
    /// <summary>
    /// The X-coordinate of the lower-right corner of this <see cref="Rectangle"/> instance
    /// </summary>
    public readonly int Right => unchecked(X + Width);
    /// <summary>
    /// The Y-coordinate of the lower-right corner of this <see cref="Rectangle"/> instance
    /// </summary>
    public readonly int Bottom => unchecked(Y + Height);

    /// <summary>
    /// Initializes a new instance of <see cref="Rectangle"/>
    /// </summary>
    /// <param name="x">The X-coordinate of the upper-left corner</param>
    /// <param name="y">The Y-coordinate of the upper-left corner</param>
    /// <param name="width">The width</param>
    /// <param name="height">The height</param>
    [SetsRequiredMembers]
    public Rectangle(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}