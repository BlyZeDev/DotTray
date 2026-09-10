namespace DotTray.Popup.Default.Context;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using DotTray.Primitives;

/// <summary>
/// Includes data for measuring <see cref="MenuItemBase"/> instances
/// </summary>
public sealed class MeasuringContext : Context
{
    internal MeasuringContext(nint gdip, float scale) : base(gdip, scale) { }

    /// <summary>
    /// Measures the size, in pixels, required to render <paramref name="text"/> with <paramref name="fontInfo"/>
    /// </summary>
    /// <param name="text">The text to measure</param>
    /// <param name="fontInfo">The font information to measure</param>
    /// <returns><see cref="SizeF"/></returns>
    public SizeF MeasureText(string text, FontInfo fontInfo)
    {
        PInvoke.GdipCreateFontFamilyFromName(fontInfo.FontFamilyName, nint.Zero, out var hFamily);
        PInvoke.GdipCreateFont(hFamily, fontInfo.Size, 0, PInvoke.UnitPixel, out var hFont);

        PInvoke.GdipCreateStringFormat(0, 0, out var hFormat);
        PInvoke.GdipSetStringFormatFlags(hFormat, PInvoke.StringFormatFlagsFitBlackBox | PInvoke.StringFormatFlagsNoWrap);
        PInvoke.GdipSetStringFormatAlign(hFormat, (int)fontInfo.Alignment);
        PInvoke.GdipSetStringFormatLineAlign(hFormat, PInvoke.StringAlignmentCenter);

        var layoutRect = new RECTF
        {
            X = 0,
            Y = 0,
            Width = float.MaxValue,
            Height = float.MaxValue
        };

        text = SanitizeText(text);
        PInvoke.GdipSetSmoothingMode(_gdip, PInvoke.SmoothingModeAntiAlias8x8);
        PInvoke.GdipSetPixelOffsetMode(_gdip, PInvoke.PixelOffsetModeHalf);
        PInvoke.GdipSetTextRenderingHint(_gdip, GetTextRenderingHint(fontInfo.Size));
        PInvoke.GdipMeasureString(_gdip, text, text.Length, hFont, ref layoutRect, hFormat, out var measured, out _, out _);

        PInvoke.GdipDeleteStringFormat(hFormat);
        PInvoke.GdipDeleteFont(hFont);
        PInvoke.GdipDeleteFontFamily(hFamily);

        return new SizeF(measured.Width, measured.Height);
    }
}