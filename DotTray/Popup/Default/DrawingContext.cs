namespace DotTray.Popup.Default;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using DotTray.Popup.Default.Coloring;
using DotTray.Primitives;
using System;
using System.ComponentModel;

/// <summary>
/// Includes data for drawing <see cref="MenuItemBase"/> instances
/// </summary>
public sealed class DrawingContext : IDisposable
{
    private readonly nint _gdip;

    /// <summary>
    /// The raw GDI+ graphics handle backing this context
    /// </summary>
    /// <remarks>
    /// Use this if you want native control over the drawing process.<br/>
    /// <b>Use with caution</b>
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public nint NativeGraphicsHandle => _gdip;

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

    /// <summary>
    /// The DPI scale factor of the monitor the menu is being shown on (1.0 = 96 DPI)
    /// </summary>
    public float Scale { get; }

    internal DrawingContext(nint gdip, float scale, Rectangle windowBounds)
    {
        _gdip = gdip;
        Scale = scale;
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
        using (var hBrush = color.CreateNativeHandle(rect))
        {
            PInvoke.GdipSetSmoothingMode(_gdip, PInvoke.SmoothingModeHighSpeed);
            PInvoke.GdipFillRectangleI(_gdip, hBrush.DangerousGetHandle(), rect.X, rect.Y, rect.Width, rect.Height);
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
        PInvoke.GdipSetStringFormatFlags(hFormat, PInvoke.StringFormatFlagsNoWrap);
        PInvoke.GdipSetStringFormatAlign(hFormat, PInvoke.StringAlignmentNear);
        PInvoke.GdipSetStringFormatLineAlign(hFormat, PInvoke.StringAlignmentCenter);

        using (var hBrush = color.CreateNativeHandle(rect))
        {
            var layoutRect = new RECTF
            {
                X = rect.X,
                Y = rect.Y,
                Width = rect.Width,
                Height = rect.Height
            };
            text = SanitizeText(text);

            PInvoke.GdipSetTextRenderingHint(_gdip, PInvoke.TextRenderingHintAntiAliasGridFit);
            PInvoke.GdipDrawString(_gdip, text, text.Length, hFont, ref layoutRect, hFormat, hBrush.DangerousGetHandle());
        }

        PInvoke.GdipDeleteStringFormat(hFormat);
        PInvoke.GdipDeleteFont(hFont);
        PInvoke.GdipDeleteFontFamily(hFamily);
    }

    void IDisposable.Dispose()
    {

    }

    private static string SanitizeText(string text) => text.Replace("\uFE0F", "");
}