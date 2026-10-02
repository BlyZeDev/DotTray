namespace DotTray.Primitives;

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary>
/// Represents 2-dimensional coordinates using integer
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 8)]
public readonly record struct Pos
{
    /// <summary>
    /// The default <see cref="Pos"/> instance
    /// </summary>
    public static readonly Pos Empty = default;

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

    /// <summary>
    /// Converts <see cref="Pos"/> to <see cref="Point"/> without allocating
    /// </summary>
    /// <param name="pos">The position to convert</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Point(Pos pos) => Unsafe.BitCast<Pos, Point>(pos);
}