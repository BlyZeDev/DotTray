namespace DotTray.Primitives;

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary>
/// Represents 2-dimensional coordinates using floating point
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 8)]
public readonly record struct PosF
{
    /// <summary>
    /// The default <see cref="PosF"/> instance
    /// </summary>
    public static readonly PosF Empty = default;

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

    /// <summary>
    /// Implicitly converts <see cref="Pos"/> to <see cref="PosF"/>
    /// </summary>
    /// <param name="pos">The position to convert</param>
    public static implicit operator PosF(Pos pos) => new PosF(pos.X, pos.Y);

    /// <summary>
    /// Converts <see cref="PosF"/> to <see cref="PointF"/> without allocating
    /// </summary>
    /// <param name="posF">The position to convert</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator PointF(PosF posF) => Unsafe.BitCast<PosF, PointF>(posF);
}