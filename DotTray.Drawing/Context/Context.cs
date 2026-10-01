namespace DotTray.Drawing.Context;

using DotTray.Internal.Native;
using System;
using System.Drawing;

/// <summary>
/// Represents the context base
/// </summary>
public abstract class Context : IDisposable
{
    /// <summary>
    /// The graphics object backing up this context
    /// </summary>
    public Graphics Graphics { get; }

    /// <summary>
    /// The DPI scale factor of the monitor the menu is being shown on (1.0 = 96 DPI)
    /// </summary>
    /// <remarks>
    /// The scale is already accounted for so this value is mostly informational
    /// </remarks>
    public float DpiScale { get; }

    internal Context(Graphics graphics, float scale)
    {
        Graphics = graphics;
        DpiScale = scale;
    }

    internal virtual void DisposeCore() { }

    void IDisposable.Dispose()
    {
        DisposeCore();
        GC.SuppressFinalize(this);
    }

    internal static int GetTextRenderingHint(float fontSize)
    {
        const float Threshold = 20f;

        return fontSize <= Threshold ? PInvoke.TextRenderingHintClearTypeGridFit : PInvoke.TextRenderingHintAntiAlias;
    }

    internal static string SanitizeText(string text) => text.Replace("\uFE0F", "");
}