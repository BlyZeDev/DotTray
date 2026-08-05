namespace DotTray.Popup.Default;

using DotTray.Popup.Default.Coloring;
using DotTray.Popup.Default.Context;
using DotTray.Primitives;
using System;

/// <summary>
/// Represents a basic popup menu item
/// </summary>
public class MenuItem : MenuItemBase
{
    private bool isHovering;

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
    /// The background hover color
    /// </summary>
    public IColorable BackgroundHover
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = SolidColor.Gray with { A = 127 };

    /// <summary>
    /// The foreground hover color
    /// </summary>
    public IColorable ForegroundHover
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
    /// The background disabled color
    /// </summary>
    public IColorable BackgroundDisabled
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
    /// The foreground disabled color
    /// </summary>
    public IColorable ForegroundDisabled
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = SolidColor.Gray;

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
    /// <see langword="true"/> to disable this instance, otherwise <see langword="false"/>
    /// </summary>
    public bool IsDisabled
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = false;

    /// <summary>
    /// Default configuration for <see cref="MenuItem"/>
    /// </summary>
    public MenuItem() { }

    /// <inheritdoc/>
    internal protected override Size Measure(MeasuringContext context)
    {
        var text = context.MeasureText(Text, FontInfo);
        return new Size((int)MathF.Ceiling(text.Width * 1.05f), (int)MathF.Ceiling(text.Height * 1.05f));
    }

    /// <inheritdoc/>
    internal protected override void Draw(DrawingContext context)
    {
        var background = IsDisabled ? BackgroundDisabled : (isHovering ? BackgroundHover : Background);
        var foreground = IsDisabled ? ForegroundDisabled : (isHovering ? ForegroundHover : Foreground);

        context.Fill(background);
        context.Write(Text, FontInfo, foreground);
    }

    /// <inheritdoc/>
    internal protected override void OnInteraction(ItemInteractedEventArgs args)
    {
        switch (args.Type)
        {
            case ItemInteractionType.MouseEnter: isHovering = true; Update(); break;
            case ItemInteractionType.MouseLeave: isHovering = false; Update(); break;
        }

        base.OnInteraction(args);
    }
}