namespace DotTray.Direct.Context;

using DotTray.Direct.Coloring;
using DotTray.Direct.Imaging;
using DotTray.Direct.Internal.Models;
using DotTray.Direct.Internal.Rendering;
using DotTray.Direct.Items;
using DotTray.Direct.Primitives;
using DotTray.Direct.Typography;
using System.ComponentModel;
using System.Runtime.CompilerServices;

/// <summary>
/// Includes data for drawing <see cref="MenuItemBase"/> instances
/// </summary>
public sealed class DrawingContext : Context
{
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

    internal DrawingContext(DirectSurface surface, Dim windowSize) : base(surface)
    {
        _brushContext = new DirectBrushContext(_surface);
        WindowSize = windowSize;
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
    /// Fills <paramref name="bounds"/> with <paramref name="brush"/>
    /// </summary>
    /// <typeparam name="TBrush">The type of brush to use</typeparam>
    /// <param name="bounds">The bounds to fill</param>
    /// <param name="brush">The brush to use</param>
    public void FillRect<TBrush>(RectF bounds, TBrush brush) where TBrush : IDirectBrush
    {
        using (var scope = brush.Create(_brushContext, bounds))
        {
            _surface.FillRect(bounds, scope.Native);
        }
    }

    /// <summary>
    /// Draws the outline of <paramref name="bounds"/> with <paramref name="brush"/>
    /// </summary>
    /// <typeparam name="TBrush">The type of brush to use</typeparam>
    /// <param name="bounds">The bounds to outline</param>
    /// <param name="brush">The brush to use</param>
    /// <param name="thickness">The border thickness, in DIPs</param>
    public void DrawRect<TBrush>(RectF bounds, TBrush brush, float thickness = 1f) where TBrush : IDirectBrush
    {
        using (var scope = brush.Create(_brushContext, bounds))
        {
            _surface.DrawRect(bounds, scope.Native, thickness);
        }
    }

    /// <summary>
    /// Fills <paramref name="bounds"/> with <paramref name="brush"/> using rounded corners
    /// </summary>
    /// <typeparam name="TBrush">The type of brush to use</typeparam>
    /// <param name="bounds">The bounds to fill</param>
    /// <param name="radius">The corner radius to use</param>
    /// <param name="brush">The brush to use</param>
    public void FillRoundedRect<TBrush>(RectF bounds, float radius, TBrush brush) where TBrush : IDirectBrush
    {
        using (var scope = brush.Create(_brushContext, bounds))
        {
            _surface.FillRoundedRect(bounds, radius, scope.Native);
        }
    }

    /// <summary>
    /// Draws the outline of <paramref name="bounds"/> with <paramref name="brush"/> using rounded corners
    /// </summary>
    /// <typeparam name="TBrush">The type of brush to use</typeparam>
    /// <param name="bounds">The bounds to outline</param>
    /// <param name="radius">The corner radius to use</param>
    /// <param name="brush">The brush to use</param>
    /// <param name="thickness">The border thickness, in DIPs</param>
    public void DrawRoundedRect<TBrush>(RectF bounds, float radius, TBrush brush, float thickness = 1f) where TBrush : IDirectBrush
    {
        using (var scope = brush.Create(_brushContext, bounds))
        {
            _surface.DrawRoundedRect(bounds, radius, scope.Native, thickness);
        }
    }

    /// <summary>
    /// Fills <paramref name="bounds"/> with <paramref name="brush"/> as an ellipse
    /// </summary>
    /// <typeparam name="TBrush">The type of brush to use</typeparam>
    /// <param name="bounds">The bounds to fill</param>
    /// <param name="brush">The brush to use</param>
    public void FillEllipse<TBrush>(RectF bounds, TBrush brush) where TBrush : IDirectBrush
    {
        using (var scope = brush.Create(_brushContext, bounds))
        {
            _surface.FillEllipse(bounds, scope.Native);
        }
    }

    /// <summary>
    /// Draws the outline of <paramref name="bounds"/> with <paramref name="brush"/> as an ellipse
    /// </summary>
    /// <typeparam name="TBrush">The type of brush to use</typeparam>
    /// <param name="bounds">The bounds to outline</param>
    /// <param name="brush">The brush to use</param>
    /// <param name="thickness">The border thickness, in DIPs</param>
    public void DrawEllipse<TBrush>(RectF bounds, TBrush brush, float thickness = 1f) where TBrush : IDirectBrush
    {
        using (var scope = brush.Create(_brushContext, bounds))
        {
            _surface.DrawEllipse(bounds, scope.Native, thickness);
        }
    }

    /// <summary>
    /// Draws a line from <paramref name="start"/> to <paramref name="end"/> with <paramref name="brush"/>
    /// </summary>
    /// <typeparam name="TBrush">The type of brush to use</typeparam>
    /// <param name="start">The start of the line</param>
    /// <param name="end">The end of the line</param>
    /// <param name="brush">The brush to use</param>
    /// <param name="thickness">The stroke thickness, in DIPs</param>
    public void DrawLine<TBrush>(PosF start, PosF end, TBrush brush, float thickness = 1f) where TBrush : IDirectBrush
    {
        var bounds = BoundsOf(start, end, thickness);

        using (var scope = brush.Create(_brushContext, bounds))
        {
            _surface.DrawLine(start, end, Math.Max(0.5f, thickness), scope.Native);
        }
    }

    /// <summary>
    /// Draws connected lines through <paramref name="points"/> with <paramref name="brush"/>
    /// </summary>
    /// <typeparam name="TBrush">The type of brush to use</typeparam>
    /// <param name="points">The points to connect, at least two</param>
    /// <param name="brush">The brush to use</param>
    /// <param name="thickness">The stroke thickness, in DIPs</param>
    public void DrawPolyline<TBrush>(ReadOnlySpan<PosF> points, TBrush brush, float thickness = 1f) where TBrush : IDirectBrush
    {
        if (points.Length < 2) return;

        float minX = points[0].X, minY = points[0].Y, maxX = minX, maxY = minY;

        var native = new PointF[points.Length];
        for (var i = 0; i < points.Length; i++)
        {
            native[i] = new PointF
            {
                X = points[i].X,
                Y = points[i].Y
            };

            minX = Math.Min(minX, points[i].X);
            minY = Math.Min(minY, points[i].Y);
            maxX = Math.Max(maxX, points[i].X);
            maxY = Math.Max(maxY, points[i].Y);
        }

        using (var scope = brush.Create(_brushContext, BoundsOf(new PosF(minX, minY), new PosF(maxX, maxY), thickness)))
        {
            _surface.DrawPolyline(native, Math.Max(0.5f, thickness), scope.Native);
        }
    }

    /// <summary>
    /// Draws a checkmark inside <paramref name="bounds"/> with <paramref name="brush"/>
    /// </summary>
    /// <remarks>
    /// The checkmark is stretched to <paramref name="bounds"/>. It looks best with bounds about 4:3 wide
    /// </remarks>
    /// <typeparam name="TBrush">The type of brush to use</typeparam>
    /// <param name="bounds">The bounds to draw in</param>
    /// <param name="brush">The brush to use</param>
    /// <param name="thickness">The stroke thickness, or <c>0f</c> to derive it from the size of <paramref name="bounds"/></param>
    public void DrawCheckmark<TBrush>(RectF bounds, TBrush brush, float thickness = 0f) where TBrush : IDirectBrush
    {
        var stroke = thickness > 0f ? thickness : Math.Max(1.5f, bounds.Height / 7f);
        var half = stroke / 2f;

        var left = bounds.X + half;
        var top = bounds.Y + half;
        var width = bounds.Width - stroke;
        var height = bounds.Height - stroke;

        var start = Point(left, top + height * 0.44f);
        var tip = Point(left + width * 0.35f, top + height);
        var end = Point(left + width, top);

        using (var scope = brush.Create(_brushContext, bounds))
        {
            _surface.DrawPolyline([start, tip, end], stroke, scope.Native);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static PointF Point(float x, float y) => new PointF
        {
            X = x,
            Y = y
        };
    }

    /// <summary>
    /// Draws a chevron arrow inside <paramref name="bounds"/> with <paramref name="brush"/>
    /// </summary>
    /// <typeparam name="TBrush">The type of brush to use</typeparam>
    /// <param name="bounds">The bounds to draw in</param>
    /// <param name="brush">The brush to use</param>
    /// <param name="direction">The direction the chevron points to</param>
    /// <param name="thickness">The stroke thickness, or <c>0f</c> to derive it from the size of <paramref name="bounds"/></param>
    public void DrawChevron<TBrush>(RectF bounds, TBrush brush, Direction direction = Direction.Right, float thickness = 0f) where TBrush : IDirectBrush
    {
        var span = direction is Direction.Left or Direction.Right ? bounds.Height : bounds.Width;
        var stroke = thickness > 0f ? thickness : Math.Max(1f, span / 10f);
        var half = stroke / 2f;

        var left = bounds.X + half;
        var top = bounds.Y + half;
        var right = bounds.X + bounds.Width - half;
        var bottom = bounds.Y + bounds.Height - half;
        var centerX = (left + right) / 2f;
        var centerY = (top + bottom) / 2f;

        var (start, tip, end) = direction switch
        {
            Direction.Left => (Point(right, top), Point(left, centerY), Point(right, bottom)),
            Direction.Up => (Point(left, bottom), Point(centerX, top), Point(right, bottom)),
            Direction.Down => (Point(left, top), Point(centerX, bottom), Point(right, top)),
            _ => (Point(left, top), Point(right, centerY), Point(left, bottom))
        };

        using (var scope = brush.Create(_brushContext, bounds))
        {
            _surface.DrawPolyline([start, tip, end], stroke, scope.Native);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static PointF Point(float x, float y) => new PointF
        {
            X = x,
            Y = y
        };
    }

    /// <summary>
    /// Draws <paramref name="image"/> stretched to <paramref name="bounds"/>
    /// </summary>
    /// <remarks>
    /// The image is not scaled proportionally, use <see cref="DirectImage.Dimensions"/> to compute bounds that keep its aspect ratio
    /// </remarks>
    /// <param name="image">The image to draw</param>
    /// <param name="bounds">The bounds to draw the image into</param>
    /// <param name="options">The options to draw the image with, or <see langword="null"/> for <see cref="ImageOptions.Default"/></param>
    public void DrawImage(DirectImage image, RectF bounds, ImageOptions? options = null) => _surface.DrawImage(image, bounds, options ?? ImageOptions.Default);

    /// <summary>
    /// Fills the whole <see cref="ItemBounds"/> with <paramref name="text"/> using <paramref name="textInfo"/> and <paramref name="brush"/>
    /// </summary>
    /// <typeparam name="TBrush">The type of brush to use</typeparam>
    /// <param name="text">The text to write</param>
    /// <param name="textInfo">The text information to use</param>
    /// <param name="brush">The brush to use</param>
    public void Write<TBrush>(string text, TextInfo textInfo, TBrush brush) where TBrush : IDirectBrush => WriteRect(ItemBounds, text, textInfo, brush);

    /// <summary>
    /// Fills the whole <paramref name="bounds"/> with <paramref name="text"/> using <paramref name="textInfo"/> and <paramref name="brush"/>
    /// </summary>
    /// <remarks>
    /// The text is vertically centered and clipped to <paramref name="bounds"/>
    /// </remarks>
    /// <typeparam name="TBrush">The type of brush to use</typeparam>
    /// <param name="bounds">The bounds to fill</param>
    /// <param name="text">The text to write</param>
    /// <param name="textInfo">The text information to use</param>
    /// <param name="brush">The brush to use</param>
    public void WriteRect<TBrush>(RectF bounds, string text, TextInfo textInfo, TBrush brush) where TBrush : IDirectBrush => _surface.DrawText(text, textInfo, bounds, brush, _brushContext);

    private static RectF BoundsOf(PosF start, PosF end, float thickness)
    {
        var pad = thickness / 2f;

        var left = MathF.Floor(Math.Min(start.X, end.X) - pad);
        var top = MathF.Floor(Math.Min(start.Y, end.Y) - pad);
        var right = MathF.Ceiling(Math.Max(start.X, end.X) + pad);
        var bottom = MathF.Ceiling(Math.Max(start.Y, end.Y) + pad);

        return new RectF(left, top, Math.Max(1, right - left), Math.Max(1, bottom - top));
    }
}