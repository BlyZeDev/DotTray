namespace DotTray.Popup.Default;

using DotTray.Popup.Default.Common;
using System.Drawing;

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
        get => field;
        set
        {
            if (string.Equals(field, value, System.StringComparison.Ordinal)) return;

            field = value;
            Update();
        }
    }

    /// <summary>
    /// The font info used to display the text
    /// </summary>
    public FontInfo FontInfo
    {
        get => field;
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
    internal protected override SizeF Measure(MeasuringContext context)
    {
        return context.MeasureText(Text, FontInfo);
    }

    /// <inheritdoc/>
    internal protected override void Draw(DrawingContext context)
    {
        context.Fill(TrayColor.Red);
        context.Write(Text, FontInfo, TrayColor.Blue);
    }
}