namespace DotTray.Primitives;

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary>
/// Represents 2-dimensional size using floating point
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 8)]
public readonly record struct DimF
{
    /// <summary>
    /// The default <see cref="DimF"/> instance
    /// </summary>
    public static readonly DimF Empty = default;

    /// <summary>
    /// The X-coordinate of this <see cref="DimF"/> instance
    /// </summary>
    public required readonly float Width { get; init; }

    /// <summary>
    /// The Y-coordinate of this <see cref="DimF"/> instance
    /// </summary>
    public required readonly float Height { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="DimF"/>
    /// </summary>
    /// <param name="width">The width</param>
    /// <param name="height">The height</param>
    [SetsRequiredMembers]
    public DimF(float width, float height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Implicitly converts <see cref="Dim"/> to <see cref="DimF"/>
    /// </summary>
    /// <param name="dim">The dimensions to convert</param>
    public static implicit operator DimF(Dim dim) => new DimF(dim.Width, dim.Height);

    /// <summary>
    /// Converts <see cref="DimF"/> to <see cref="SizeF"/> without allocating
    /// </summary>
    /// <param name="dimF">The dimensions to convert</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator SizeF(DimF dimF) => Unsafe.BitCast<DimF, SizeF>(dimF);
}