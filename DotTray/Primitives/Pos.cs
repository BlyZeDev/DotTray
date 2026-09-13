namespace DotTray.Primitives;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

/// <summary>
/// Represents 2-dimensional coordinates using integer
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct Pos
{
    /// <summary>
    /// The X-coordinate of this <see cref="Pos"/> instance
    /// </summary>
    public required readonly int X { get; init; }

    /// <summary>
    /// The Y-coordinate of this <see cref="Pos"/> instance
    /// </summary>
    public required readonly int Y { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="Pos"/>
    /// </summary>
    /// <param name="x">The X-coordinate</param>
    /// <param name="y">The Y-coordinate</param>
    [SetsRequiredMembers]
    public Pos(int x, int y)
    {
        X = x;
        Y = y;
    }
}