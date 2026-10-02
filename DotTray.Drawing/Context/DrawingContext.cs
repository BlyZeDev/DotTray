namespace DotTray.Drawing.Context;

using DotTray.Drawing;
using DotTray.Drawing.Primitives;
using DotTray.Primitives;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary>
/// Includes data for drawing <see cref="MenuItemBase"/> instances
/// </summary>
public sealed class DrawingContext : Context
{
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

    internal DrawingContext(Graphics graphics, float scale, Rect windowBounds) : base(graphics, scale)
    {
        WindowSize = new Dim(windowBounds.Right - windowBounds.Left, windowBounds.Bottom - windowBounds.Top);
    }

    /// <summary>
    /// Draws a checkmark inside <paramref name="bounds"/> with <paramref name="brush"/>
    /// </summary>
    /// <typeparam name="TBrush">The brush type to use</typeparam>
    /// <param name="bounds">The bounds to draw inside</param>
    /// <param name="brush">The brush to use</param>
    public void DrawCheckmark<TBrush>(Rect bounds, TBrush brush) where TBrush : notnull, Brush
    {
        var tVert = Math.Max(2, bounds.Height / 4);
        tVert += tVert % 2;
        var tVertHalf = tVert / 2;

        var shortArm = Math.Max(tVertHalf + 2, bounds.Width * 35 / 100);
        var longArm = Math.Max(tVertHalf + 5, bounds.Width * 64 / 100);

        if (shortArm + longArm > bounds.Width)
        {
            longArm = bounds.Width - shortArm;
        }

        var left = bounds.X + (bounds.Width - (shortArm + longArm)) / 2;
        var top = bounds.Y + tVertHalf + (bounds.Height - (longArm + tVertHalf)) / 2;

        var p0X = left;
        var p0Y = top + longArm - shortArm;

        var p1X = left + shortArm;
        var p1Y = top + longArm;

        var p2X = left + shortArm + longArm;

        ReadOnlySpan<Pos> checkmark =
        [
            new Pos(p0X, p0Y),
            new Pos(p1X, p1Y),
            new Pos(p2X, top),
            new Pos(p2X - tVertHalf, top - tVertHalf),
            new Pos(p1X, p1Y - tVert),
            new Pos(p0X + tVertHalf, p0Y - tVertHalf)
        ];

        FillPolygon(brush, checkmark);
    }

    /// <summary>
    /// Draws a chevron arrow inside <paramref name="bounds"/> with <paramref name="brush"/>
    /// </summary>
    /// <typeparam name="TBrush">The brush type to use</typeparam>
    /// <param name="bounds">The bounds to draw inside</param>
    /// <param name="brush">The brush to use</param>
    public void DrawChevron<TBrush>(Rect bounds, TBrush brush) where TBrush : notnull, Brush
    {
        var thickness = Math.Max(1, bounds.Height / 5);
        var centerY = bounds.Y + bounds.Height / 2;

        ReadOnlySpan<Pos> arrow =
        [
            new Pos(bounds.X, bounds.Y),
            new Pos(bounds.X + thickness, bounds.Y),
            new Pos(bounds.X + bounds.Width, centerY),
            new Pos(bounds.X + thickness, bounds.Y + bounds.Height),
            new Pos(bounds.X, bounds.Y + bounds.Height),
            new Pos(bounds.X + bounds.Width - thickness, centerY)
        ];

        FillPolygon(brush, arrow);
    }

    /// <summary>
    /// Fills the whole <see cref="ItemBounds"/> with <paramref name="text"/> using <paramref name="fontInfo"/> and <paramref name="brush"/>
    /// </summary>
    /// <typeparam name="TBrush">The brush type to use</typeparam>
    /// <param name="text">The text to write</param>
    /// <param name="fontInfo">The font information to use</param>
    /// <param name="brush">The brush to use</param>
    public void Write<TBrush>(string text, FontInfo fontInfo, TBrush brush) where TBrush : notnull, Brush
        => WriteRect(ItemBounds, text, fontInfo, brush);

    /// <summary>
    /// Fills the whole <paramref name="rect"/> with <paramref name="text"/> using <paramref name="fontInfo"/> and <paramref name="brush"/>
    /// </summary>
    /// <typeparam name="TBrush">The brush type to use</typeparam>
    /// <param name="rect">The rectangle to fill</param>
    /// <param name="text">The text to write</param>
    /// <param name="fontInfo">The font information to use</param>
    /// <param name="brush">The brush to use</param>
    public void WriteRect<TBrush>(RectF rect, string text, FontInfo fontInfo, TBrush brush) where TBrush : notnull, Brush
    {
        using (var font = CreateFont(fontInfo))
        {
            using (var format = CreateFormat(fontInfo))
            {
                text = SanitizeText(text);

                Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                Graphics.TextRenderingHint = GetTextRenderingHint(fontInfo.Size);
                Graphics.DrawString(text, font, brush, Unsafe.BitCast<RectF, RectangleF>(rect), format);
            }
        }
    }

    private void FillPolygon(Brush brush, ReadOnlySpan<Pos> polygon)
    {
        Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        Graphics.FillPolygon(brush, MemoryMarshal.Cast<Pos, Point>(polygon), FillMode.Alternate);
    }
}