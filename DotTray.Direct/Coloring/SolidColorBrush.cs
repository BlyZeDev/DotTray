namespace DotTray.Direct.Coloring;

using DotTray.Direct.Context;
using DotTray.Direct.Primitives;

/// <summary>
/// Represents a solid color brush
/// </summary>
public sealed class SolidColorBrush : IDirectBrush
{
    /// <inheritdoc cref="Rgba.Transparent"/>
    public static readonly SolidColorBrush Transparent = new SolidColorBrush(Rgba.Transparent);
    /// <inheritdoc cref="Rgba.Black"/>
    public static readonly SolidColorBrush Black = new SolidColorBrush(Rgba.Black);
    /// <inheritdoc cref="Rgba.White"/>
    public static readonly SolidColorBrush White = new SolidColorBrush(Rgba.White);
    /// <inheritdoc cref="Rgba.Red"/>
    public static readonly SolidColorBrush Red = new SolidColorBrush(Rgba.Red);
    /// <inheritdoc cref="Rgba.Green"/>
    public static readonly SolidColorBrush Green = new SolidColorBrush(Rgba.Green);
    /// <inheritdoc cref="Rgba.Blue"/>
    public static readonly SolidColorBrush Blue = new SolidColorBrush(Rgba.Blue);

    /// <summary>
    /// The color of this instance
    /// </summary>
    public Rgba Color { get; }

    /// <summary>
    /// Initializes a new <see cref="SolidColorBrush"/> instance
    /// </summary>
    /// <param name="color">The color to use</param>
    public SolidColorBrush(Rgba color) => Color = color;

    /// <inheritdoc/>
    public DirectBrush Create(DirectBrushContext context, Rect bounds) => new DirectBrush(context._surface.CreateSolidBrush(Color));
}