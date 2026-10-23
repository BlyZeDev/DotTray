namespace DotTray.Direct.Context;

using DotTray.Direct;
using DotTray.Direct.Coloring;
using DotTray.Direct.Internal.Rendering;
using DotTray.Direct.Items;
using DotTray.Direct.Primitives;
using DotTray.Direct.Typography;

/// <summary>
/// Includes data for drawing <see cref="MenuItemBase"/> instances
/// </summary>
public sealed class DrawingContext : Context
{
    private readonly DirectSurface _surface;
    private readonly DirectBrushContext _brushContext;

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
        _brushContext = new DirectBrushContext(_surface);
        WindowSize = windowSize;
    }

    /// <summary>
    /// Fills <paramref name="bounds"/> with <paramref name="brush"/>
    /// </summary>
    /// <typeparam name="TBrush">The type of brush to use</typeparam>
    /// <param name="bounds">The bounds to fill</param>
    /// <param name="brush">The brush to use</param>
    public void FillRect<TBrush>(Rect bounds, TBrush brush) where TBrush : IDirectBrush
    {
        using (var scope = brush.Create(_brushContext, bounds))
        {
            _surface.FillRect(bounds, scope.Native);
        }
    }

    /// <summary>
    /// Fills <paramref name="bounds"/> with <paramref name="brush"/> using rounded corners
    /// </summary>
    /// <typeparam name="TBrush">The type of brush to use</typeparam>
    /// <param name="bounds">The bounds to fill</param>
    /// <param name="radius">The corner radius to use</param>
    /// <param name="brush">The brush to use</param>
    public void FillRoundedRect<TBrush>(Rect bounds, float radius, TBrush brush) where TBrush : IDirectBrush
    {
        using (var scope = brush.Create(_brushContext, bounds))
        {
            _surface.FillRoundedRect(bounds, radius, scope.Native);
        }
    }

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
    /// Draws a checkmark inside <paramref name="bounds"/> with <paramref name="brush"/>
    /// </summary>
    /// <typeparam name="TBrush">The type of brush to use</typeparam>
    /// <param name="bounds">The bounds to draw in</param>
    /// <param name="brush">The brush to use</param>
    public void DrawCheckmark<TBrush>(Rect bounds, TBrush brush) where TBrush : IDirectBrush
    {
        var stroke = Math.Max(1.5f, bounds.Height / 8f);

        var x0 = bounds.X + bounds.Width * 0.12f;
        var y0 = bounds.Y + bounds.Height * 0.52f;
        var x1 = bounds.X + bounds.Width * 0.40f;
        var y1 = bounds.Y + bounds.Height * 0.80f;
        var x2 = bounds.X + bounds.Width * 0.88f;
        var y2 = bounds.Y + bounds.Height * 0.22f;

        using (var scope = brush.Create(_brushContext, bounds))
        {
            _surface.DrawLine(x0, y0, x1, y1, stroke, scope.Native);
            _surface.DrawLine(x1, y1, x2, y2, stroke, scope.Native);
        }
    }

    /// <summary>
    /// Draws a chevron arrow inside <paramref name="bounds"/> with <paramref name="brush"/>
    /// </summary>
    /// <typeparam name="TBrush">The type of brush to use</typeparam>
    /// <param name="bounds">The bounds to draw in</param>
    /// <param name="brush">The brush to use</param>
    public void DrawChevron<TBrush>(Rect bounds, TBrush brush) where TBrush : IDirectBrush
    {
        var stroke = Math.Max(1f, bounds.Height / 10f);
        var centerY = bounds.Y + bounds.Height / 2f;

        using (var scope = brush.Create(_brushContext, bounds))
        {
            _surface.DrawLine(bounds.X, bounds.Y, bounds.X + bounds.Width, centerY, stroke, scope.Native);
            _surface.DrawLine(bounds.X + bounds.Width, centerY, bounds.X, bounds.Y + bounds.Height, stroke, scope.Native);
        }
    }

    /// <summary>
    /// Fills the whole <see cref="ItemBounds"/> with <paramref name="text"/> using <paramref name="fontInfo"/> and <paramref name="brush"/>
    /// </summary>
    /// <typeparam name="TBrush">The type of brush to use</typeparam>
    /// <param name="text">The text to write</param>
    /// <param name="fontInfo">The font information to use</param>
    /// <param name="brush">The brush to use</param>
    public void Write<TBrush>(string text, FontInfo fontInfo, TBrush brush) where TBrush : IDirectBrush => WriteRect(ItemBounds, text, fontInfo, brush);

    /// <summary>
    /// Fills the whole <paramref name="bounds"/> with <paramref name="text"/> using <paramref name="fontInfo"/> and <paramref name="brush"/>
    /// </summary>
    /// <remarks>
    /// The text is vertically centered and clipped to <paramref name="bounds"/>
    /// </remarks>
    /// <typeparam name="TBrush">The type of brush to use</typeparam>
    /// <param name="bounds">The bounds to fill</param>
    /// <param name="text">The text to write</param>
    /// <param name="fontInfo">The font information to use</param>
    /// <param name="brush">The brush to use</param>
    public void WriteRect<TBrush>(Rect bounds, string text, FontInfo fontInfo, TBrush brush) where TBrush : IDirectBrush
    {
        using (var scope = brush.Create(_brushContext, bounds))
        {
            _surface.DrawText(text, fontInfo, bounds, scope.Native);
        }
    }
}