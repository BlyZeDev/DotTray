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
    }

    /// <summary>
    /// The font info used to display the text
    /// </summary>
    public FontInfo FontInfo
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            Update();
        }
    }

    /// <summary>
    /// Default configuration for <see cref="MenuItem"/>
    /// </summary>
    public MenuItem()
    {
        Text = "";
        FontInfo = new FontInfo
        {
            FontFamilyName = "Segoe UI Emoji",
            Size = 16f
        };
    }

    /// <inheritdoc/>
    internal protected override Size Measure(MeasuringContext context)
    {
        var text = context.MeasureText(Text, FontInfo);
        return new Size((int)MathF.Ceiling(text.Width), (int)MathF.Ceiling(text.Height));
    }

    /// <inheritdoc/>
    internal protected override void Draw(DrawingContext context)
    {
        var color = LinearGradientColor.Random();

        context.Fill(color);
        context.Write(Text, FontInfo, LinearGradientColor.Random());
    }
}