namespace DotTray.Popup.Default;

using DotTray.Popup.Default.Coloring;
using DotTray.Primitives;
using System;

/// <summary>
/// Represents a basic popup menu item that includes a submenu
/// </summary>
public class SubmenuItem : SubmenuItemBase
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
    /// Default configuration for <see cref="SubmenuItem"/>
    /// </summary>
    public SubmenuItem() { }

    /// <inheritdoc/>
    internal protected override Size Measure(MeasuringContext context)
    {
        var text = context.MeasureText(Text, FontInfo);
        var arrow = context.MeasureText("\u276F", FontInfo);

        var width = text.Width + arrow.Width + (12f * context.Scale);
        var height = MathF.Max(text.Height, arrow.Height);

        return new Size((int)MathF.Ceiling(width), (int)MathF.Ceiling(height));
    }

    /// <inheritdoc/>
    internal protected override void Draw(DrawingContext context)
    {
        context.Fill(Background);

        var bounds = context.ItemBounds;
        var arrowWidth = MathF.Ceiling(bounds.Height);

        var textRect = new Rectangle(bounds.X, bounds.Y, bounds.Width - (int)arrowWidth, bounds.Height);
        var arrowRect = new Rectangle(bounds.Right - (int)arrowWidth, bounds.Y, (int)arrowWidth, bounds.Height);

        context.WriteRect(textRect, Text, FontInfo, Foreground);
        context.WriteRect(arrowRect, "\u276F", FontInfo, Foreground);
    }

    /// <inheritdoc/>
    protected internal override void OnInteraction(ItemInteractedEventArgs args)
    {
        Console.WriteLine("SUBMENUITEM: " + args);
    }
}