namespace DotTray.Drawing.Context;

using DotTray.Drawing;
using DotTray.Drawing.Primitives;
using DotTray.Primitives;
using System.Drawing;
using System.Drawing.Drawing2D;

/// <summary>
/// Includes data for measuring <see cref="MenuItemBase"/> instances
/// </summary>
public sealed class MeasuringContext : Context
{
    internal MeasuringContext(Graphics graphics, float scale) : base(graphics, scale) { }

    /// <summary>
    /// Measures the size, in pixels, required to render <paramref name="text"/> with <paramref name="fontInfo"/>
    /// </summary>
    /// <param name="text">The text to measure</param>
    /// <param name="fontInfo">The font information to measure the text in</param>
    /// <returns><see cref="DimF"/></returns>
    public DimF MeasureText(string text, FontInfo fontInfo)
    {
        using (var font = CreateFont(fontInfo))
        {
            using (var format = CreateFormat(fontInfo))
            {
                text = SanitizeText(text);

                Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                Graphics.TextRenderingHint = GetTextRenderingHint(fontInfo.Size);

                var measured = Graphics.MeasureString(text, font, new SizeF(float.MaxValue, float.MaxValue), format);

                return new DimF(measured.Width, measured.Height);
            }
        }
    }
}