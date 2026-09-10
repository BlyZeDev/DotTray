namespace DotTray.Popup.Default.Items;

using DotTray.Internal.Native;
using DotTray.Popup.Default;
using DotTray.Popup.Default.Coloring;
using DotTray.Popup.Default.Context;
using DotTray.Primitives;
using System;

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
    internal protected override Size Measure(MeasuringContext context)
    {
        var text = context.MeasureText(Text, FontInfo);
        var baseSize = new Size((int)MathF.Ceiling(text.Width * 1.05f), (int)MathF.Ceiling(text.Height * 1.05f));

        if (SubmenuItems.IsEmpty) return baseSize;

        var arrowHeight = Math.Max(6, baseSize.Height / ArrowHeightRatio);
        var arrowWidth = Math.Max(4, arrowHeight * 2 / 3);

        return new Size(baseSize.Width + ArrowGap + arrowWidth + ArrowRightPadding, baseSize.Height);
    }

    /// <inheritdoc/>
    internal protected override void Draw(DrawingContext context)
    {
        var background = IsDisabled ? BackgroundDisabled : (isHovering ? BackgroundHover : Background);
        var foreground = IsDisabled ? ForegroundDisabled : (isHovering ? ForegroundHover : Foreground);

        var bounds = context.ItemBounds;

        if (SubmenuItems.IsEmpty)
        {
            context.FillRect(bounds, background);
            context.Write(Text, FontInfo, foreground);
            return;
        }

        var arrowHeight = Math.Max(6, bounds.Height / ArrowHeightRatio);
        var arrowWidth = Math.Max(4, arrowHeight * 2 / 3);

        var arrowX = bounds.Right - ArrowRightPadding - arrowWidth;
        var arrowY = bounds.Top + (bounds.Height - arrowHeight) / 2;

        var textBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width - arrowWidth - ArrowGap - ArrowRightPadding, bounds.Height);
        var arrowBounds = new Rectangle(arrowX, arrowY, arrowWidth, arrowHeight);

        context.FillRect(bounds, background);
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