namespace DotTray.Direct.Coloring;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a color at a position of a gradient
/// </summary>
public readonly record struct GradientStop
{
    /// <summary>
    /// The color of the stop
    /// </summary>
    public required readonly Rgba Color { get; init; }

    /// <summary>
    /// The offset of the stop
    /// </summary>
    /// <remarks>
    /// This value is clamped between <c>0</c> and <c>1</c>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public required readonly float Offset
    {
        get;
        init
        {
            if (float.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(Offset), $"Offset must not be {nameof(float.NaN)}");

            field = Math.Clamp(value, 0f, 1f);
        }
    }

    /// <summary>
    /// Initializes a new <see cref="GradientStop"/>
    /// </summary>
    /// <param name="color">The color of the stop</param>
    /// <param name="offset">The position clamped between <c>0</c> and <c>1</c></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [SetsRequiredMembers]
    public GradientStop(Rgba color, float offset)
    {
        Color = color;
        Offset = offset;
    }
}