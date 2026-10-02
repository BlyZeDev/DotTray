namespace DotTray.Drawing.Context;

using DotTray.Drawing.Primitives;
using System;
using System.Drawing;
using System.Drawing.Text;

/// <summary>
/// Represents the context base
/// </summary>
public abstract class Context : IDisposable
{
    /// <summary>
    /// The graphics object backing up this context
    /// </summary>
    /// <remarks>
    /// Do not call <see cref="IDisposable.Dispose"/> on it or wrap it in a <c>using</c> block;
    /// it is disposed when the context is disposed.
    /// </remarks>
    public Graphics Graphics { get; }

    /// <summary>
    /// The DPI scale factor of the monitor the menu is being shown on (1.0 = 96 DPI)
    /// </summary>
    /// <remarks>
    /// The scale is already accounted for so this value is mostly informational
    /// </remarks>
    public float Scale { get; }

    internal Context(Graphics graphics, float scale)
    {
        Graphics = graphics;
        Scale = scale;
    }

    internal virtual void DisposeCore() { }

    void IDisposable.Dispose()
    {
        DisposeCore();
        GC.SuppressFinalize(this);
    }

    internal static Font CreateFont(FontInfo info) => new Font(info.FontFamily, info.Size, info.Style, GraphicsUnit.Pixel);

    internal static StringFormat CreateFormat(FontInfo info)
    {
        return new StringFormat
        {
            Alignment = info.Alignment,
            FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoWrap,
            LineAlignment = StringAlignment.Center
        };
    }

    internal static TextRenderingHint GetTextRenderingHint(float fontSize)
    {
        const float Threshold = 20f;

        return fontSize <= Threshold ? TextRenderingHint.ClearTypeGridFit : TextRenderingHint.AntiAlias;
    }

    internal static string SanitizeText(string text) => text.Replace("\uFE0F", "");
}