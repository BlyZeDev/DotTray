namespace DotTray.Primitives;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents 2-dimensional size using floating point
/// </summary>
public readonly record struct SizeF
{
    /// <summary>
    /// The default <see cref="SizeF"/> instance
    /// </summary>
    public static readonly SizeF Empty = default;

    /// <summary>
    /// The X-coordinate of this <see cref="SizeF"/> instance
    /// </summary>
    public required readonly float Width { get; init; }

    /// <summary>
    /// The Y-coordinate of this <see cref="SizeF"/> instance
    /// </summary>
    public required readonly float Height { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="SizeF"/>
    /// </summary>
    /// <param name="width">The width</param>
    /// <param name="height">The height</param>
    [SetsRequiredMembers]
    public SizeF(float width, float height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Implicitly converts <see cref="Size"/> to <see cref="SizeF"/>
    /// </summary>
    /// <param name="size">The size to convert</param>
    public static implicit operator SizeF(Size size) => new SizeF(size.Width, size.Height);
}