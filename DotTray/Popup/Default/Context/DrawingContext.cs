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
    /// Fills the whole <see cref="ItemBounds"/> with <paramref name="color"/>
    /// </summary>
    /// <typeparam name="TColor">The color type to use</typeparam>
    /// <param name="color">The color to use</param>
    public void Fill<TColor>(TColor color) where TColor : notnull, IColorable
        => FillRect(ItemBounds, color);

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
    /// Fills the an ellipse inside <paramref name="rect"/> with <paramref name="color"/>
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

    public void DrawImage(Rectangle rect, ImageSource image)
    {
        PInvoke.GdipDrawImageRectI(_gdip, image.Handle, rect.X, rect.Y, rect.Width, rect.Height);
    }

    private static Rectangle GetBounds(ReadOnlySpan<Point> points)
    {
        var minX = points[0].X;
        var minY = points[0].Y;
        var maxX = points[0].X;
        var maxY = points[0].Y;

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