namespace DotTray;

using DotTray.Internal.Native;
using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a <see cref="NotifyIcon"/> menu item
/// </summary>
public sealed class MenuItem : IMenuItem
{
    private static readonly Rgb DefaultBackgroundColor = new Rgb(255, 255, 255);
    private static readonly Rgb DefaultBackgroundHoverColor = new Rgb(0, 120, 215);
    private static readonly Rgb DefaultBackgroundDisabledColor = new Rgb(255, 255, 255);
    private static readonly Rgb DefaultTextColor = new Rgb(0, 0, 0);
    private static readonly Rgb DefaultTextHoverColor = new Rgb(255, 255, 255);
    private static readonly Rgb DefaultTextDisabledColor = new Rgb(109, 109, 109);

    private string text;
    private Rgb backgroundColor;
    private Rgb backgroundHoverColor;
    private Rgb backgroundDisabledColor;
    private Rgb textColor;
    private Rgb textHoverColor;
    private Rgb textDisabledColor;

    internal uint fState;

    internal event Action? Changed;

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
            Update();
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

            Update();
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
            
            Update();
        }
    }

    /// <summary>
    /// The default background color
    /// </summary>
    public Rgb BackgroundColor
    {
        get => backgroundColor;
        set
        {
            if (backgroundColor == value) return;

            backgroundColor = value;
            Update();
        }
    }

    /// <summary>
    /// The background color if this item is hovered over
    /// </summary>
    public Rgb BackgroundHoverColor
    {
        get => backgroundHoverColor;
        set
        {
            if (backgroundHoverColor == value) return;

            backgroundHoverColor = value;
            Update();
        }
    }

    /// <summary>
    /// The background color if this item is disabled
    /// </summary>
    public Rgb BackgroundDisabledColor
    {
        get => backgroundDisabledColor;
        set
        {
            if (backgroundDisabledColor == value) return;

            backgroundDisabledColor = value;
            Update();
        }
    }

    /// <summary>
    /// The default text color
    /// </summary>
    public Rgb TextColor
    {
        get => textColor;
        set
        {
            if (textColor == value) return;

            textColor = value;
            Update();
        }
    }

    /// <summary>
    /// The text color if this item is hovered over
    /// </summary>
    public Rgb TextHoverColor
    {
        get => textHoverColor;
        set
        {
            if (textHoverColor == value) return;

            textHoverColor = value;
            Update();
        }
    }

    /// <summary>
    /// The text color if this item is disabled
    /// </summary>
    public Rgb TextDisabledColor
    {
        get => textDisabledColor;
        set
        {
            if (textDisabledColor == value) return;

            textDisabledColor = value;
            Update();
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
        Rgb backgroundColor,
        Rgb backgroundHoverColor,
        Rgb backgroundDisabledColor,
        Rgb textColor,
        Rgb textHoverColor,
        Rgb textDisabledColor,
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

    private void Update() => Changed?.Invoke();
}