namespace DotTray.Popup.Default.Coloring;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using DotTray.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

/// <summary>
/// Represents a two-color linear gradient
/// </summary>
public readonly record struct LinearGradientColor : IColorable
{
    /// <summary>
    /// The color at the start of the gradient
    /// </summary>
    public required readonly SolidColor Start { get; init; }
    /// <summary>
    /// The color at the end of the gradient
    /// </summary>
    public required readonly SolidColor End { get; init;  }

    /// <summary>
    /// The angle, in degrees, of the gradient line
    /// </summary>
    public float Angle { get; init; } = 0f;

    /// <summary>
    /// Initializes a new <see cref="LinearGradientColor"/>
    /// </summary>
    /// <param name="start">The start color</param>
    /// <param name="end">The end color</param>
    /// <param name="angle">The angle of the gradient in degrees</param>
    [SetsRequiredMembers]
    public LinearGradientColor(SolidColor start, SolidColor end, float angle)
    {
        Start = start;
        End = end;
        Angle = angle;
    }

    /// <inheritdoc/>
    public bool Equals(IColorable? other) => other is LinearGradientColor gradient && Equals(gradient);

    SafeHandle IColorable.CreateNativeHandle(RectangleF bounds)
    {
        var rect = new RECTF
        {
            X = bounds.X,
            Y = bounds.Y,
            Width = bounds.Width,
            Height = bounds.Height
        };

        PInvoke.GdipCreateLineBrushFromRectWithAngle(ref rect, ToGdip(Start), ToGdip(End), Angle, true, 0, out var hBrush);

        return new ColorSafeHandle(hBrush);
    }

    /// <summary>
    /// Generates a <see cref="LinearGradientColor"/> with random <see cref="Start"/>, <see cref="End"/> and <see cref="Angle"/>
    /// </summary>
    /// <param name="randomAlpha"><see langword="true"/> if the alpha value should be random as well, otherwise <see langword="false"/></param>
    /// <returns><see cref="LinearGradientColor"/></returns>
    public static LinearGradientColor Random(bool randomAlpha = false)
        => new LinearGradientColor(SolidColor.Random(randomAlpha), SolidColor.Random(randomAlpha), System.Random.Shared.NextSingle() * 360);

    private static uint ToGdip(SolidColor color) => (uint)(color.A << 24 | color.R << 16 | color.G << 8 | color.B);
}