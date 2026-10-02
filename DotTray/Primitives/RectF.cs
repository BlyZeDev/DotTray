namespace DotTray.Primitives;

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary>
/// Represents a location and size using floating point
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 16)]
public readonly record struct RectF
{
    /// <summary>
    /// The default <see cref="RectF"/> instance
    /// </summary>
    public static readonly RectF Empty = default;

    /// <summary>
    /// The X-coordinate of the upper-left corner of this <see cref="RectF"/> instance
    /// </summary>
    public required readonly float X { get; init; }
    /// <summary>
    /// The Y-coordinate of the upper-left corner of this <see cref="RectF"/> instance
    /// </summary>
    public required readonly float Y { get; init; }
    /// <summary>
    /// The Width of this <see cref="RectF"/> instance
    /// </summary>
    public required readonly float Width { get; init; }
    /// <summary>
    /// The Height of this <see cref="RectF"/> instance
    /// </summary>
    public required readonly float Height { get; init; }

    /// <summary>
    /// <inheritdoc cref="X"/>
    /// </summary>
    public readonly float Left => X;
    /// <summary>
    /// <inheritdoc cref="Y"/>
    /// </summary>
    public readonly float Top => Y;
    /// <summary>
    /// The X-coordinate of the lower-right corner of this <see cref="RectF"/> instance
    /// </summary>
    public readonly float Right => unchecked(X + Width);
    /// <summary>
    /// The Y-coordinate of the lower-right corner of this <see cref="RectF"/> instance
    /// </summary>
    public readonly float Bottom => unchecked(Y + Height);

    /// <summary>
    /// Initializes a new instance of <see cref="RectF"/>
    /// </summary>
    /// <param name="x">The X-coordinate of the upper-left corner</param>
    /// <param name="y">The Y-coordinate of the upper-left corner</param>
    /// <param name="width">The width</param>
    /// <param name="height">The height</param>
    [SetsRequiredMembers]
    public RectF(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Implicitly converts <see cref="Rect"/> to <see cref="RectF"/>
    /// </summary>
    /// <param name="rect">The rectangle to convert</param>
    public static implicit operator RectF(Rect rect) => new RectF(rect.X, rect.Y, rect.Width, rect.Height);

    /// <summary>
    /// Converts <see cref="RectF"/> to <see cref="RectangleF"/> without allocating
    /// </summary>
    /// <param name="rectF">The rectangle to convert</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator RectangleF(RectF rectF) => Unsafe.BitCast<RectF, RectangleF>(rectF);
}