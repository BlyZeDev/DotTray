namespace DotTray;

using DotTray.Internal.Native;
using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a <see cref="NotifyIcon"/> menu item
/// </summary>
public sealed class MenuItem : IMenuItem
{
    private static readonly TrayColor DefaultBackgroundColor = new TrayColor(255, 255, 255);
    private static readonly TrayColor DefaultBackgroundHoverColor = new TrayColor(0, 120, 215);
    private static readonly TrayColor DefaultBackgroundDisabledColor = new TrayColor(255, 255, 255);
    private static readonly TrayColor DefaultTextColor = new TrayColor(0, 0, 0);
    private static readonly TrayColor DefaultTextHoverColor = new TrayColor(255, 255, 255);
    private static readonly TrayColor DefaultTextDisabledColor = new TrayColor(109, 109, 109);

    private string text;
    private TrayColor backgroundColor;
    private TrayColor backgroundHoverColor;
    private TrayColor backgroundDisabledColor;
    private TrayColor textColor;
    private TrayColor textHoverColor;
    private TrayColor textDisabledColor;

    internal uint fState;
    internal bool HasSubMenu => SubMenu.Count > 0;

    /// <summary>
    /// The displayed text
    /// </summary>
    public required string Text
    {
        get => text;
        [MemberNotNull(nameof(text))]
        set
        {
            if (text == value) return;

            text = value;
        }
    }

    /// <summary>
    /// <see langword="true"/> if this <see cref="MenuItem"/> is checked, otherwise <see langword="false"/>
    /// <see langword="null"/> if this <see cref="MenuItem"/> is not checkable
    /// </summary>
    public bool? IsChecked
    {
        get => (fState & PInvoke.MFS_CHECKED) != 0;
        set
        {
            if ((fState & PInvoke.MFS_CHECKED) != 0 == value) return;

            if (value.GetValueOrDefault()) fState |= PInvoke.MFS_CHECKED;
            else fState &= ~PInvoke.MFS_CHECKED;
        }
    }

    /// <summary>
    /// <see langword="true"/> if this <see cref="MenuItem"/> is disabled, otherwise <see langword="false"/>
    /// </summary>
    public bool IsDisabled
    {
        get => (fState & PInvoke.MFS_DISABLED) != 0;
        set
        {
            if ((fState & PInvoke.MFS_DISABLED) != 0 == value) return;

            if (value) fState |= PInvoke.MFS_DISABLED;
            else fState &= ~PInvoke.MFS_DISABLED;
        }
    }

    /// <summary>
    /// The default background color
    /// </summary>
    public TrayColor BackgroundColor
    {
        get => backgroundColor;
        set
        {
            if (backgroundColor == value) return;

            backgroundColor = value;
        }
    }

    /// <summary>
    /// The background color if this item is hovered over
    /// </summary>
    public TrayColor BackgroundHoverColor
    {
        get => backgroundHoverColor;
        set
        {
            if (backgroundHoverColor == value) return;

            backgroundHoverColor = value;
        }
    }

    /// <summary>
    /// The background color if this item is disabled
    /// </summary>
    public TrayColor BackgroundDisabledColor
    {
        get => backgroundDisabledColor;
        set
        {
            if (backgroundDisabledColor == value) return;

            backgroundDisabledColor = value;
        }
    }

    /// <summary>
    /// The default text color
    /// </summary>
    public TrayColor TextColor
    {
        get => textColor;
        set
        {
            if (textColor == value) return;

            textColor = value;
        }
    }

    /// <summary>
    /// The text color if this item is hovered over
    /// </summary>
    public TrayColor TextHoverColor
    {
        get => textHoverColor;
        set
        {
            if (textHoverColor == value) return;

            textHoverColor = value;
        }
    }

    /// <summary>
    /// The text color if this item is disabled
    /// </summary>
    public TrayColor TextDisabledColor
    {
        get => textDisabledColor;
        set
        {
            if (textDisabledColor == value) return;

            textDisabledColor = value;
        }
    }

    /// <summary>
    /// The <see cref="Action{MenuItemInteractionArgs}"/> to invoke if this <see cref="MenuItem"/> is interacted with
    /// </summary>
    public Action<MenuItemClickedArgs>? Clicked { get; set; }

    /// <summary>
    /// The submenu for this <see cref="MenuItem"/>
    /// </summary>
    public MenuItemCollection SubMenu { get; }

    [SetsRequiredMembers]
    internal MenuItem(string text) : this(text,
        null,
        false,
        DefaultBackgroundColor,
        DefaultBackgroundHoverColor,
        DefaultBackgroundDisabledColor,
        DefaultTextColor,
        DefaultTextHoverColor,
        DefaultTextDisabledColor,
        []) { }

    [SetsRequiredMembers]
    internal MenuItem(
        string text,
        bool? isChecked,
        bool isDisabled,
        TrayColor backgroundColor,
        TrayColor backgroundHoverColor,
        TrayColor backgroundDisabledColor,
        TrayColor textColor,
        TrayColor textHoverColor,
        TrayColor textDisabledColor,
        MenuItemCollection subMenu)
    {
        Text = text;
        IsChecked = isChecked;
        IsDisabled = isDisabled;
        BackgroundColor = backgroundColor;
        BackgroundHoverColor = backgroundHoverColor;
        BackgroundDisabledColor = backgroundDisabledColor;
        TextColor = textColor;
        TextHoverColor = textHoverColor;
        TextDisabledColor = textDisabledColor;
        SubMenu = subMenu;
    }
}