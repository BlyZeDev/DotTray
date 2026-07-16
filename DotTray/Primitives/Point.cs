namespace DotTray.Primitives;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents 2-dimensional coordinates
/// </summary>
public readonly record struct Point
{
    /// <summary>
    /// The X-coordinate of this <see cref="Point"/> instance
    /// </summary>
    public required readonly int X { get; init; }

    /// <summary>
    /// The Y-coordinate of this <see cref="Point"/> instance
    /// </summary>
    public required readonly int Y { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="Point"/>
    /// </summary>
    /// <param name="x">The X-coordinate</param>
    /// <param name="y">The Y-coordinate</param>
    [SetsRequiredMembers]
    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }
}