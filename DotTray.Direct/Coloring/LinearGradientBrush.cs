namespace DotTray.Direct.Coloring;

using DotTray.Direct.Context;
using DotTray.Direct.Internal.Models;
using DotTray.Direct.Primitives;


/// <summary>
/// Represents a linear gradient brush
/// </summary>
public sealed class LinearGradientBrush : IDirectBrush
{
    private readonly GradientStop[] _stops;

    /// <summary>
    /// Start point relative to the bounds <c>(0..1)</c>
    /// </summary>
    public PosF Start { get; }

    /// <summary>
    /// End point relative to the bounds <c>(0..1)</c>
    /// </summary>
    public PosF End { get; }

    /// <summary>
    /// The behaviour outside the gradient range
    /// </summary>
    public GradientExtendMode ExtendMode { get; }

    /// <summary>
    /// Initializes a new <see cref="LinearGradientBrush"/> instance
    /// </summary>
    /// <param name="stops">The gradient stops (at least one)</param>
    /// <param name="start">Start point relative to the bounds, default top-left</param>
    /// <param name="end">End point relative to the bounds, default bottom-right</param>
    /// <param name="extendMode">The behavior outside the gradient range</param>
    public LinearGradientBrush(IEnumerable<GradientStop> stops, PosF? start = null, PosF? end = null, GradientExtendMode extendMode = GradientExtendMode.Clamp)
    {
        _stops = Sort(stops);
        Start = start ?? new PosF(0, 0);
        End = end ?? new PosF(1, 1);
        ExtendMode = extendMode;
    }

    /// <summary>
    /// Creates a gradient running in a CSS-like <paramref name="degrees"/> direction<br/>
    /// (0 = bottom to top, 90 = left to right, 180 = top to bottom)
    /// </summary>
    /// <param name="stops">The gradient stops (at least one)</param>
    /// <param name="degrees">The degrees of the gradient angle</param>
    /// <param name="extendMode">The behavior outside the gradient range</param>
    /// <returns><see cref="LinearGradientBrush"/></returns>
    public static LinearGradientBrush FromAngle(IEnumerable<GradientStop> stops, float degrees, GradientExtendMode extendMode = GradientExtendMode.Clamp)
    {
        var radius = degrees * MathF.PI / 180f;
        var dx = MathF.Sin(radius) * 0.5f;
        var dy = -MathF.Cos(radius) * 0.5f;

        return new LinearGradientBrush(stops, new PosF(0.5f - dx, 0.5f - dy), new PosF(0.5f + dx, 0.5f + dy), extendMode);
    }

    /// <summary>
    /// Creates a top to bottom gradient
    /// </summary>
    /// <param name="top">The top color</param>
    /// <param name="bottom">The bottom color</param>
    /// <returns><see cref="LinearGradientBrush"/></returns>
    public static LinearGradientBrush Vertical(Rgba top, Rgba bottom)
    {
        var stops = new GradientStop[]
        {
            new GradientStop(top, 0f),
            new GradientStop(bottom, 1f)
        };

        return new LinearGradientBrush(stops, new PosF(0f, 0f), new PosF(0f, 1f));
    }

    /// <summary>
    /// Creates a left to right gradient
    /// </summary>
    /// <param name="left">The left color</param>
    /// <param name="right">The right color</param>
    /// <returns><see cref="LinearGradientBrush"/></returns>
    public static LinearGradientBrush Horizontal(Rgba left, Rgba right)
    {
        var stops = new GradientStop[]
        {
            new GradientStop(left, 0f),
            new GradientStop(right, 1f)
        };

        return new LinearGradientBrush(stops, new PosF(0f, 0f), new PosF(1f, 0f));
    }

    /// <inheritdoc/>
    public DirectBrush Create(DirectBrushContext context, RectF bounds)
    {
        var start = new PointF
        {
            X = bounds.X + Start.X * bounds.Width,
            Y = bounds.Y + Start.Y * bounds.Height
        };
        var end = new PointF
        {
            X = bounds.X + End.X * bounds.Width,
            Y = bounds.Y + End.Y * bounds.Height
        };

        return new DirectBrush(context._surface.CreateLinearGradientBrush(_stops, start, end, ExtendMode));
    }

    private static GradientStop[] Sort(IEnumerable<GradientStop> stops)
    {
        var array = stops.OrderBy(static x => x.Offset).ToArray();
        return array.Length == 0 ? throw new ArgumentException("At least one gradient stop is required", nameof(stops)) : array;
    }
}