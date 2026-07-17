namespace DotTray.Popup.Default;

using DotTray.Popup.Default.Coloring;
using DotTray.Primitives;
using System;

/// <summary>
/// Represents a basic popup menu item
/// </summary>
public class MenuItem : MenuItemBase
{
    /// <summary>
    /// The background color
    /// </summary>
    public IColorable Background
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = SolidColor.Transparent;

    /// <summary>
    /// The text color
    /// </summary>
    public IColorable Foreground
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = SolidColor.Black;

    /// <summary>
    /// The displayed text
    /// </summary>
    public string Text
    {
        get;
        set
        {
            if (string.Equals(field, value, StringComparison.Ordinal)) return;

            field = value;
            Update();
        }
    } = "";

    /// <summary>
    /// The font info used to display the text
    /// </summary>
    public FontInfo FontInfo
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = new FontInfo("Segoe UI Emoji", 20f);

    /// <summary>
    /// Default configuration for <see cref="MenuItem"/>
    /// </summary>
    public MenuItem() { }

    /// <inheritdoc/>
    internal protected override Size Measure(MeasuringContext context)
    {
        var text = context.MeasureText(Text, FontInfo);
        return new Size((int)MathF.Ceiling(text.Width), (int)MathF.Ceiling(text.Height));
    }

    /// <inheritdoc/>
    internal protected override void Draw(DrawingContext context)
    {
        context.Fill(Background);
        context.Write(Text, FontInfo, Foreground);
    }
}