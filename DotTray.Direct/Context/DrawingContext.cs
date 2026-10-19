namespace DotTray.Direct.Context;

using DotTray.Direct;
using DotTray.Direct.Internal.Rendering;
using DotTray.Direct.Items;
using DotTray.Direct.Primitives;
using System.Drawing;

/// <summary>
/// Includes data for drawing <see cref="MenuItemBase"/> instances
/// </summary>
public sealed class DrawingContext : Context
{
    private readonly DirectSurface _surface;

    /// <summary>
    /// The size of the window that contains this item
    /// </summary>
    public Dim WindowSize { get; }

    /// <summary>
    /// The bounds, in window client coordinates, assigned to the item currently being drawn
    /// </summary>
    /// <remarks>
    /// This is set immediately before each item's <see cref="MenuItemBase.Draw(DrawingContext)"/> is called
    /// </remarks>
    public Rect ItemBounds { get; internal set; }

    internal DrawingContext(DirectSurface surface, float scale, Dim windowSize) : base(scale)
    {
        _surface = surface;
        WindowSize = windowSize;
    }

    /// <summary>
    /// Fills <paramref name="bounds"/> with <paramref name="color"/>
    /// </summary>
    /// <param name="bounds">The bounds to fill</param>
    /// <param name="color">The color to use</param>
    public void FillRect(Rect bounds, Color color) => _surface.FillRect(bounds, color);

    /// <summary>
    /// Fills <paramref name="bounds"/> with <paramref name="color"/> using rounded corners
    /// </summary>
    /// <param name="bounds">The bounds to fill</param>
    /// <param name="radius">The corner radius in DIPs</param>
    /// <param name="color">The color to use</param>
    public void FillRoundedRect(Rect bounds, float radius, Color color) => _surface.FillRoundedRect(bounds, radius, color);

    /// <summary>
    /// Clips all drawing to <paramref name="bounds"/> until the returned scope is disposed
    /// </summary>
    /// <param name="bounds">The bounds to clip to</param>
    /// <returns><see cref="ClipScope"/></returns>
    public ClipScope Clip(Rect bounds)
    {
        _surface.PushClip(bounds);
        return new ClipScope(_surface);
    }

    /// <summary>
    /// Draws a checkmark inside <paramref name="bounds"/> with <paramref name="color"/>
    /// </summary>
    /// <param name="bounds">The bounds to draw inside</param>
    /// <param name="color">The color to use</param>
    public void DrawCheckmark(Rect bounds, Color color)
    {
        var stroke = Math.Max(1.5f, bounds.Height / 8f);

        var x0 = bounds.X + bounds.Width * 0.12f;
        var y0 = bounds.Y + bounds.Height * 0.52f;
        var x1 = bounds.X + bounds.Width * 0.40f;
        var y1 = bounds.Y + bounds.Height * 0.80f;
        var x2 = bounds.X + bounds.Width * 0.88f;
        var y2 = bounds.Y + bounds.Height * 0.22f;

        _surface.DrawLine(x0, y0, x1, y1, stroke, color);
        _surface.DrawLine(x1, y1, x2, y2, stroke, color);
    }

    /// <summary>
    /// Draws a chevron arrow inside <paramref name="bounds"/> with <paramref name="color"/>
    /// </summary>
    /// <param name="bounds">The bounds to draw inside</param>
    /// <param name="color">The color to use</param>
    public void DrawChevron(Rect bounds, Color color)
    {
        var stroke = Math.Max(1f, bounds.Height / 10f);
        var centerY = bounds.Y + bounds.Height / 2f;

        _surface.DrawLine(bounds.X, bounds.Y, bounds.X + bounds.Width, centerY, stroke, color);
        _surface.DrawLine(bounds.X + bounds.Width, centerY, bounds.X, bounds.Y + bounds.Height, stroke, color);
    }

    /// <summary>
    /// Fills the whole <see cref="ItemBounds"/> with <paramref name="text"/> using <paramref name="fontInfo"/> and <paramref name="color"/>
    /// </summary>
    /// <param name="text">The text to write</param>
    /// <param name="fontInfo">The font information to use</param>
    /// <param name="color">The color to use</param>
    public void Write(string text, FontInfo fontInfo, Color color) => WriteRect(ItemBounds, text, fontInfo, color);

    /// <summary>
    /// Fills the whole <paramref name="rect"/> with <paramref name="text"/> using <paramref name="fontInfo"/> and <paramref name="color"/>
    /// </summary>
    /// <remarks>
    /// The text is vertically centered and clipped to <paramref name="rect"/>
    /// </remarks>
    /// <param name="rect">The rectangle to fill</param>
    /// <param name="text">The text to write</param>
    /// <param name="fontInfo">The font information to use</param>
    /// <param name="color">The color to use</param>
    public void WriteRect(Rect rect, string text, FontInfo fontInfo, Color color) => _surface.DrawText(text, fontInfo, rect, color);
}