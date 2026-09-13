namespace DotTray.Primitives;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents 2-dimensional coordinates using floating point
/// </summary>
public readonly record struct PosF
{
    /// <summary>
    /// The X-coordinate of this <see cref="PosF"/> instance
    /// </summary>
    public required readonly float X { get; init; }

    /// <summary>
    /// The Y-coordinate of this <see cref="PosF"/> instance
    /// </summary>
    public required readonly float Y { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="PosF"/>
    /// </summary>
    /// <param name="x">The X-coordinate</param>
    /// <param name="y">The Y-coordinate</param>
    [SetsRequiredMembers]
    public PosF(float x, float y)
    {
        X = x;
        Y = y;
    }
}