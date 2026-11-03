namespace DotTray.Direct.Coloring;

using DotTray.Direct.Context;
using DotTray.Direct.Internal.Models;
using DotTray.Direct.Primitives;

/// <summary>
/// Represents a radial gradient brush
/// </summary>
public sealed class RadialGradientBrush : IDirectBrush
{
    private readonly GradientStop[] _stops;

    /// <summary>
    /// Center relative to the bounds <c>(0..1)</c>
    /// </summary>
    public PosF Center { get; }

    /// <summary>
    /// Radius relative to the bounds width/height <c>(0.5f = half the width/height)</c>
    /// </summary>
    public PosF Radius { get; }

    /// <summary>
    /// Gradient origin offset from the center, relative to the bounds size
    /// </summary>
    public PosF OriginOffset { get; }

    /// <summary>
    /// The behavior outside the gradient range
    /// </summary>
    public GradientExtendMode ExtendMode { get; }

    /// <summary>
    /// Initializes a new <see cref="RadialGradientBrush"/> instance
    /// </summary>
    /// <param name="stops">The gradient stops (at least one)</param>
    /// <param name="center">Center relative to the bounds, default the middle</param>
    /// <param name="radius">Radius relative to the width/height</param>
    /// <param name="originOffset">Offsets the focal point from the center</param>
    /// <param name="extendMode">The behavior outside the gradient range</param>
    public RadialGradientBrush(IEnumerable<GradientStop> stops, PosF? center = null, PosF? radius = null, PosF? originOffset = null, GradientExtendMode extendMode = GradientExtendMode.Clamp)
    {
        _stops = Sort(stops);
        Center = center ?? new PosF(0.5f, 0.5f);
        Radius = radius ?? new PosF(0.5f, 0.5f);
        OriginOffset = originOffset ?? new PosF(0f, 0f);
        ExtendMode = extendMode;
    }

    /// <inheritdoc/>
    public DirectBrush Create(DirectBrushContext context, RectF bounds)
    {
        var center = new PointF
        {
            X = bounds.X + Center.X * bounds.Width,
            Y = bounds.Y + Center.Y * bounds.Height
        };
        var offset = new PointF
        {
            X = OriginOffset.X * bounds.Width,
            Y = OriginOffset.Y * bounds.Height
        };

        return new DirectBrush(context._surface.CreateRadialGradientBrush(_stops, center, offset, Radius.X * bounds.Width, Radius.Y * bounds.Height, ExtendMode));
    }

    private static GradientStop[] Sort(IEnumerable<GradientStop> stops)
    {
        var array = stops.OrderBy(static x => x.Offset).ToArray();
        return array.Length == 0 ? throw new ArgumentException("At least one gradient stop is required", nameof(stops)) : array;
    }
}