namespace DotTray.Primitives;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents 2-dimensional size using integer
/// </summary>
public readonly record struct Size
{
    /// <summary>
    /// The default <see cref="Size"/> instance
    /// </summary>
    public static readonly Size Empty = default;

    /// <summary>
    /// The X-coordinate of this <see cref="Size"/> instance
    /// </summary>
    public required readonly int Width { get; init; }

    /// <summary>
    /// The Y-coordinate of this <see cref="Size"/> instance
    /// </summary>
    public required readonly int Height { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="Size"/>
    /// </summary>
    /// <param name="width">The width</param>
    /// <param name="height">The height</param>
    [SetsRequiredMembers]
    public Size(int width, int height)
    {
        Width = width;
        Height = height;
    }
}