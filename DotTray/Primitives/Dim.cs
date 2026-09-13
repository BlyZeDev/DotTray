namespace DotTray.Primitives;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents 2-dimensional size using integer
/// </summary>
public readonly record struct Dim
{
    /// <summary>
    /// The default <see cref="Dim"/> instance
    /// </summary>
    public static readonly Dim Empty = default;

    /// <summary>
    /// The X-coordinate of this <see cref="Dim"/> instance
    /// </summary>
    public required readonly int Width { get; init; }

    /// <summary>
    /// The Y-coordinate of this <see cref="Dim"/> instance
    /// </summary>
    public required readonly int Height { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="Dim"/>
    /// </summary>
    /// <param name="width">The width</param>
    /// <param name="height">The height</param>
    [SetsRequiredMembers]
    public Dim(int width, int height)
    {
        Width = width;
        Height = height;
    }
}