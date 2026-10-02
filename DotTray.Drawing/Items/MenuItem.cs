namespace DotTray.Drawing.Items;

using DotTray.Drawing;
using DotTray.Drawing.Context;
using DotTray.Drawing.Primitives;
using DotTray.Primitives;
using System;
using System.Drawing;

/// <summary>
/// Represents a popup menu item
/// </summary>
public class MenuItem : MenuItemBase
{
    private const int ArrowHeightRatio = 3;
    private const int ArrowGap = 8;
    private const int ArrowRightPadding = 10;

    /// <summary>
    /// <see langword="true"/> if this instance is hovered over or focused by the keyboard navigation, otherwise <see langword="false"/>
    /// </summary>
    protected bool isHovering;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <remarks>
    /// This is the same value as <see cref="IsDisabled"/> and therefore can be ignored
    /// </remarks>
    internal protected sealed override bool IgnoreInteraction => IsDisabled;
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <remarks>
    /// This returns the same reference as <see cref="Items"/> and therefore can be ignored
    /// </remarks>
    internal protected sealed override MenuItemCollection SubmenuItems => base.SubmenuItems;

    /// <summary>
    /// The background color
    /// </summary>
    public Brush Background
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = SystemBrushes.Menu;

    /// <summary>
    /// The foreground color
    /// </summary>
    public Brush Foreground
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = SystemBrushes.MenuText;

    /// <summary>
    /// The background hover color
    /// </summary>
    public Brush BackgroundHover
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = SystemBrushes.Highlight;

    /// <summary>
    /// The foreground hover color
    /// </summary>
    public Brush ForegroundHover
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = SystemBrushes.HighlightText;

    /// <summary>
    /// The background disabled color
    /// </summary>
    public Brush BackgroundDisabled
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = SystemBrushes.Menu;

    /// <summary>
    /// The foreground disabled color
    /// </summary>
    public Brush ForegroundDisabled
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = SystemBrushes.GrayText;

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
    } = new FontInfo("Segoe UI Emoji", 12);

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
    /// Items that should be shown in a submenu popup
    /// </summary>
    /// <remarks>
    /// If <see cref="MenuItemCollection.IsEmpty"/>, no submenu popup will appear
    /// </remarks>
    public MenuItemCollection Items => SubmenuItems;

    /// <summary>
    /// Default configuration for <see cref="MenuItem"/>
    /// </summary>
    public MenuItem() { }

    /// <inheritdoc/>
    internal protected override void Initialize() => isHovering = false;

    /// <inheritdoc/>
    internal protected override Dim Measure(MeasuringContext context)
    {
        var text = context.MeasureText(Text, FontInfo);
        var baseSize = new Dim((int)MathF.Ceiling(text.Width * 1.05f), (int)MathF.Ceiling(text.Height * 1.05f));

        if (SubmenuItems.IsEmpty) return baseSize;

        var arrowHeight = Math.Max(6, baseSize.Height / ArrowHeightRatio);
        var arrowWidth = Math.Max(4, arrowHeight * 2 / 3);

        return new Dim(baseSize.Width + ArrowGap + arrowWidth + ArrowRightPadding, baseSize.Height);
    }

    /// <inheritdoc/>
    internal protected override void Draw(DrawingContext context)
    {
        var background = IsDisabled ? BackgroundDisabled : (isHovering ? BackgroundHover : Background);
        var foreground = IsDisabled ? ForegroundDisabled : (isHovering ? ForegroundHover : Foreground);

        var bounds = context.ItemBounds;

        if (SubmenuItems.IsEmpty)
        {
            context.Graphics.FillRectangle(background, bounds);
            context.Write(Text, FontInfo, foreground);
            return;
        }

        var arrowHeight = Math.Max(6, bounds.Height / ArrowHeightRatio);
        var arrowWidth = Math.Max(4, arrowHeight * 2 / 3);

        var arrowX = bounds.Right - ArrowRightPadding - arrowWidth;
        var arrowY = bounds.Top + (bounds.Height - arrowHeight) / 2;

        var textBounds = new Rect(bounds.X, bounds.Y, bounds.Width - arrowWidth - ArrowGap - ArrowRightPadding, bounds.Height);
        var arrowBounds = new Rect(arrowX, arrowY, arrowWidth, arrowHeight);

        context.Graphics.FillRectangle(background, bounds);
        context.WriteRect(textBounds, Text, FontInfo, foreground);
        context.DrawChevron(arrowBounds, foreground);
    }

    /// <inheritdoc/>
    internal protected override void OnInteraction(ItemInteractedEventArgs args)
    {
        switch (args.Type)
        {
            case ItemInteractionType.MouseEnter or ItemInteractionType.KeyboardFocus:
                isHovering = true;
                Update();
                break;

            case ItemInteractionType.MouseLeave or ItemInteractionType.KeyboardBlur:
                isHovering = false;
                Update();
                break;
        }

        base.OnInteraction(args);
    }
}