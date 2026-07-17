namespace DotTray.Popup.Default.Coloring;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a gradient color stop
/// </summary>
public readonly record struct GradientStop
{
    /// <summary>
    /// The color of the gradient stop
    /// </summary>
    public required readonly SolidColor Color { get; init; }

    /// <summary>
    /// The relative position of the gradient stop
    /// </summary>
    /// <remarks>
    /// The position is clamped between <b>0.0</b> - <b>1.0</b>.<br/>
    /// <b>0.0</b> specifies the start, while <b>1.0</b> specifies the end of the gradient
    /// </remarks>
    public required readonly float Position { get; init; }

    /// <summary>
    /// Initializes a new <see cref="GradientStop"/> instance
    /// </summary>
    /// <param name="color">The color of the gradient stop</param>
    /// <param name="position">The position of the gradient stop</param>
    [SetsRequiredMembers]
    public GradientStop(SolidColor color, float position)
    {
        Color = color;
        Position = position;
    }
}