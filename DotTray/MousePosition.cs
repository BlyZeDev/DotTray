namespace DotTray;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a 2-dimensional mouse position
/// </summary>
public readonly record struct MousePosition
{
    /// <summary>
    /// The X-coordinate
    /// </summary>
    public readonly int X { get; init; }

    /// <summary>
    /// The Y-coordinate
    /// </summary>
    public readonly int Y { get; init; }

    /// <summary>
    /// Initializes a new mouse position
    /// </summary>
    /// <param name="x">The X-coordinate</param>
    /// <param name="y">The Y-coordinate</param>
    [SetsRequiredMembers]
    public MousePosition(int x, int y)
    {
        X = x;
        Y = y;
    }
}