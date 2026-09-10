namespace DotTray.Popup.Default.Context;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using DotTray.Popup.Default;
using DotTray.Popup.Default.Coloring;
using DotTray.Primitives;
using System;

/// <summary>
/// Includes data for drawing <see cref="MenuItemBase"/> instances
/// </summary>
public sealed class DrawingContext : Context
{
    /// <summary>
    /// The size of the window that contains this item
    /// </summary>
    public Size WindowSize { get; }

    /// <summary>
    /// The bounds, in window client coordinates, assigned to the item currently being drawn
    /// </summary>
    /// <remarks>
    /// This is set immediately before each item's <see cref="MenuItemBase.Draw(DrawingContext)"/> is called
    /// </remarks>
    public Rectangle ItemBounds { get; internal set; }

    internal DrawingContext(nint gdip, float scale, Rectangle windowBounds) : base(gdip, scale)
    {
        WindowSize = new Size(windowBounds.Right - windowBounds.Left, windowBounds.Bottom - windowBounds.Top);
    }

    /// <summary>
    /// Draws the border of <paramref name="rect"/> with <paramref name="color"/>
    /// </summary>
    /// <typeparam name="TColor">The color type to use</typeparam>
    /// <param name="rect">The rectangle to outline</param>
    /// <param name="color">The color to use</param>
    /// <param name="strokeWidth">The stroke width to use</param>
    public void DrawRect<TColor>(Rectangle rect, TColor color, float strokeWidth = 2f) where TColor : notnull, IColorable
    {
        using (var hPen = color.CreateGdipPen(rect, strokeWidth))
        {
            PInvoke.GdipSetSmoothingMode(_gdip, PInvoke.SmoothingModeHighSpeed);
            PInvoke.GdipSetPenMode(hPen.DangerousGetHandle(), PInvoke.PenAlignmentInset);
            PInvoke.GdipDrawRectangleI(_gdip, hPen.DangerousGetHandle(), rect.X, rect.Y, rect.Width, rect.Height);
        }
    }

    /// <summary>
    /// Fills the whole <paramref name="rect"/> with <paramref name="color"/>
    /// </summary>
    /// <typeparam name="TColor">The color type to use</typeparam>
    /// <param name="rect">The rectangle to fill</param>
    /// <param name="color">The color to use</param>
    public void FillRect<TColor>(Rectangle rect, TColor color) where TColor : notnull, IColorable
    {
        using (var hBrush = color.CreateGdipBrush(rect))
        {
            PInvoke.GdipSetSmoothingMode(_gdip, PInvoke.SmoothingModeHighSpeed);
            PInvoke.GdipFillRectangleI(_gdip, hBrush.DangerousGetHandle(), rect.X, rect.Y, rect.Width, rect.Height);
        }
    }

    /// <summary>
    /// Draws the ellipse inside <paramref name="rect"/> with <paramref name="color"/>
    /// </summary>
    /// <typeparam name="TColor">The color type to use</typeparam>
    /// <param name="rect">The ellipse to outline</param>
    /// <param name="color">The color to use</param>
    /// <param name="strokeWidth">The stroke width to use</param>
    public void DrawEllipse<TColor>(Rectangle rect, TColor color, float strokeWidth = 2f) where TColor : notnull, IColorable
    {
        using (var hPen = color.CreateGdipPen(rect, strokeWidth))
        {
            PInvoke.GdipSetSmoothingMode(_gdip, PInvoke.SmoothingModeAntiAlias8x8);
            PInvoke.GdipSetPenMode(hPen.DangerousGetHandle(), PInvoke.PenAlignmentInset);
            PInvoke.GdipDrawEllipseI(_gdip, hPen.DangerousGetHandle(), rect.X, rect.Y, rect.Width, rect.Height);
        }
    }

    /// <summary>
    /// Fills the ellipse inside <paramref name="rect"/> with <paramref name="color"/>
    /// </summary>
    /// <typeparam name="TColor">The color type to use</typeparam>
    /// <param name="rect">The rectangle to fill with an ellipse</param>
    /// <param name="color">The color to use</param>
    public void FillEllipse<TColor>(Rectangle rect, TColor color) where TColor : notnull, IColorable
    {
        using (var hBrush = color.CreateGdipBrush(rect))
        {
            PInvoke.GdipSetSmoothingMode(_gdip, PInvoke.SmoothingModeAntiAlias8x8);
            PInvoke.GdipFillEllipseI(_gdip, hBrush.DangerousGetHandle(), rect.X, rect.Y, rect.Width, rect.Height);
        }
    }

    /// <summary>
    /// Fills a polygon defined by <paramref name="points"/> with <paramref name="color"/>
    /// </summary>
    /// <remarks>
    /// A polygon requires at least 3 points
    /// </remarks>
    /// <typeparam name="TColor">The color type to use</typeparam>
    /// <param name="color">The color to use</param>
    /// <param name="points">The points defining the polygon</param>
    public void FillPolygon<TColor>(TColor color, params ReadOnlySpan<Point> points) where TColor : notnull, IColorable
    {
        if (points.Length < 3)
        {
            return;
        }

        var bounds = GetBounds(points);

        using (var hBrush = color.CreateGdipBrush(bounds))
        {
            PInvoke.GdipSetSmoothingMode(_gdip, PInvoke.SmoothingModeAntiAlias8x8);

            unsafe
            {
                fixed (Point* hPoints = points)
                {
                    PInvoke.GdipFillPolygonI(_gdip, hBrush.DangerousGetHandle(), (POINT*)hPoints, points.Length, PInvoke.FillModeAlternate);
                }
            }
        }
    }

    /// <summary>
    /// Draws a checkmark inside <paramref name="bounds"/> with <paramref name="color"/>
    /// </summary>
    /// <typeparam name="TColor">The color type to use</typeparam>
    /// <param name="bounds">The bounds to draw inside</param>
    /// <param name="color">The color to use</param>
    public void DrawCheckmark<TColor>(Rectangle bounds, TColor color) where TColor : notnull, IColorable
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

        ReadOnlySpan<Point> checkmark =
        [
            new Point(p0X, p0Y),
            new Point(p1X, p1Y),
            new Point(p2X, top),
            new Point(p2X - tVertHalf, top - tVertHalf),
            new Point(p1X, p1Y - tVert),
            new Point(p0X + tVertHalf, p0Y - tVertHalf)
        ];

        FillPolygon(color, checkmark);
    }

    /// <summary>
    /// Draws a chevron arrow inside <paramref name="bounds"/> with <paramref name="color"/>
    /// </summary>
    /// <typeparam name="TColor">The color type to use</typeparam>
    /// <param name="bounds">The bounds to draw inside</param>
    /// <param name="color">The color to use</param>
    public void DrawChevron<TColor>(Rectangle bounds, TColor color) where TColor : notnull, IColorable
    {
        var thickness = Math.Max(1, bounds.Height / 5);
        var centerY = bounds.Y + bounds.Height / 2;

        ReadOnlySpan<Point> arrow =
        [
            new Point(bounds.X, bounds.Y),
            new Point(bounds.X + thickness, bounds.Y),
            new Point(bounds.X + bounds.Width, centerY),
            new Point(bounds.X + thickness, bounds.Y + bounds.Height),
            new Point(bounds.X, bounds.Y + bounds.Height),
            new Point(bounds.X + bounds.Width - thickness, centerY)
        ];

        FillPolygon(color, arrow);
    }

    /// <summary>
    /// Fills the whole <see cref="ItemBounds"/> with <paramref name="text"/> using <paramref name="fontInfo"/> and <paramref name="color"/>
    /// </summary>
    /// <typeparam name="TColor">The color type to use</typeparam>
    /// <param name="text">The text to write</param>
    /// <param name="fontInfo">The font information to use</param>
    /// <param name="color">The color to use</param>
    public void Write<TColor>(string text, FontInfo fontInfo, TColor color) where TColor : notnull, IColorable
        => WriteRect(ItemBounds, text, fontInfo, color);

    /// <summary>
    /// Fills the whole <paramref name="rect"/> with <paramref name="text"/> using <paramref name="fontInfo"/> and <paramref name="color"/>
    /// </summary>
    /// <typeparam name="TColor">The color type to use</typeparam>
    /// <param name="rect">The rectangle to fill</param>
    /// <param name="text">The text to write</param>
    /// <param name="fontInfo">The font information to use</param>
    /// <param name="color">The color to use</param>
    public void WriteRect<TColor>(RectangleF rect, string text, FontInfo fontInfo, TColor color) where TColor : notnull, IColorable
    {
        PInvoke.GdipCreateFontFamilyFromName(fontInfo.FontFamilyName, nint.Zero, out var hFamily);
        PInvoke.GdipCreateFont(hFamily, fontInfo.Size, 0, PInvoke.UnitPixel, out var hFont);

        PInvoke.GdipCreateStringFormat(0, 0, out var hFormat);
        PInvoke.GdipSetStringFormatFlags(hFormat, PInvoke.StringFormatFlagsFitBlackBox | PInvoke.StringFormatFlagsNoWrap);
        PInvoke.GdipSetStringFormatAlign(hFormat, (int)fontInfo.Alignment);
        PInvoke.GdipSetStringFormatLineAlign(hFormat, PInvoke.StringAlignmentCenter);

        using (var hBrush = color.CreateGdipBrush(rect))
        {
            var layoutRect = new RECTF
            {
                X = rect.X,
                Y = rect.Y,
                Width = rect.Width,
                Height = rect.Height
            };
            text = SanitizeText(text);

            PInvoke.GdipSetSmoothingMode(_gdip, PInvoke.SmoothingModeAntiAlias8x8);
            PInvoke.GdipSetPixelOffsetMode(_gdip, PInvoke.PixelOffsetModeHalf);
            PInvoke.GdipSetTextRenderingHint(_gdip, GetTextRenderingHint(fontInfo.Size));
            PInvoke.GdipDrawString(_gdip, text, text.Length, hFont, ref layoutRect, hFormat, hBrush.DangerousGetHandle());
        }

        PInvoke.GdipDeleteStringFormat(hFormat);
        PInvoke.GdipDeleteFont(hFont);
        PInvoke.GdipDeleteFontFamily(hFamily);
    }

    /// <summary>
    /// Fills the whole <see cref="ItemBounds"/> with <paramref name="image"/>
    /// </summary>
    /// <param name="image">The image to draw</param>
    public void DrawImage(ImageSource image) => DrawImageRect(ItemBounds, image);

    /// <summary>
    /// Fills the whole <paramref name="rect"/> with <paramref name="image"/>
    /// </summary>
    /// <param name="rect">The rectangle to fill</param>
    /// <param name="image">The image to draw</param>
    public void DrawImageRect(Rectangle rect, ImageSource image)
    {
        var scaled = image.GetScaled(rect.Width, rect.Height);
        PInvoke.GdipDrawImageRectI(_gdip, scaled.Handle, rect.X, rect.Y, rect.Width, rect.Height);
    }

    private static Rectangle GetBounds(ReadOnlySpan<Point> points)
    {
        var minX = points[0].X;
        var minY = points[0].Y;
        var maxX = minX;
        var maxY = minY;

        for (var i = 1; i < points.Length; i++)
        {
            minX = Math.Min(minX, points[i].X);
            minY = Math.Min(minY, points[i].Y);
            maxX = Math.Max(maxX, points[i].X);
            maxY = Math.Max(maxY, points[i].Y);
        }

        return new Rectangle(minX, minY, maxX - minX, maxY - minY);
    }
}