namespace DotTray.Popup.Default.Coloring;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using DotTray.Primitives;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

/// <summary>
/// Represents a multi-stop linear gradient
/// </summary>
public readonly record struct LinearGradientStopColor : IColorable
{
    /// <summary>
    /// The gradient stops
    /// </summary>
    public required readonly ImmutableArray<GradientStop> Stops { get; init; }

    /// <summary>
    /// The angle, in degrees, of the gradient line
    /// </summary>
    public readonly float Angle { get; init; } = 0f;

    /// <summary>
    /// Initializes a new <see cref="LinearGradientStopColor"/>
    /// </summary>
    /// <param name="angle">The angle of the gradient in degrees</param>
    /// <param name="stops">The gradient stops</param>
    [SetsRequiredMembers]
    public LinearGradientStopColor(float angle, params ImmutableArray<GradientStop> stops)
    {
        Stops = stops;
        Angle = angle;
    }

    /// <inheritdoc/>
    public readonly bool Equals(IColorable? other) => other is LinearGradientStopColor gradient && Equals(gradient);

    readonly SafeHandle IColorable.CreateNativeHandle(RectangleF bounds)
    {
        var rect = new RECTF
        {
            X = bounds.X,
            Y = bounds.Y,
            Width = bounds.Width,
            Height = bounds.Height
        };

        PInvoke.GdipCreateLineBrushFromRectWithAngle(ref rect, ToGdip(Stops[0].Color), ToGdip(Stops[^1].Color), Angle, true, 0, out var hBrush);

        unsafe
        {
            var colors = stackalloc uint[Stops.Length];
            var positions = stackalloc float[Stops.Length];
            for (var i = 0; i < Stops.Length; i++)
            {
                colors[i] = ToGdip(Stops[i].Color);
                positions[i] = Stops[i].Position;
            }

            PInvoke.GdipSetLinePresetBlend(hBrush, colors, positions, Stops.Length);
        }

        return new ColorSafeHandle(hBrush);
    }

    private static uint ToGdip(SolidColor color) => (uint)(color.A << 24 | color.R << 16 | color.G << 8 | color.B);
}