namespace DotTray;

using DotTray.Internal;
using System;

/// <summary>
/// Represents the base of a <see cref="NotifyIcon"/> menu item
/// </summary>
public record MenuItem
{
    internal string text;
    internal uint fState;
    private Rgb backgroundColor;
    private Rgb backgroundHoverColor;
    private Rgb backgroundDisabledColor;
    private Rgb textColor;
    private Rgb textHoverColor;
    private Rgb textDisabledColor;

    internal event Action? Changed;

    /// <summary>
    /// The displayed text
    /// </summary>
    public string Text
    {
        get => text;
        set
        {
            if (text == value) return;

            text = value;
            Update();
        }
    }

    /// <summary>
    /// <see langword="true"/> if this <see cref="MenuItem"/> is disabled, otherwise <see langword="false"/>
    /// </summary>
    public bool IsDisabled
    {
        get => (fState & Native.MFS_DISABLED) != 0;
        set
        {
            if ((fState & Native.MFS_DISABLED) != 0 == value) return;

            if (value) fState |= Native.MFS_DISABLED;
            else fState &= ~Native.MFS_DISABLED;
            
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
    /// The <see cref="Action{MenuItemClickedArgs}"/> to invoke if this <see cref="MenuItem"/> is clicked
    /// </summary>
    public Action<MenuItemClickedArgs>? Click { get; set; }

    internal MenuItem(string text, bool? isChecked, bool isDisabled, Rgb backgroundColor, Rgb backgroundHoverColor, Rgb backgroundDisabledColor, Rgb textColor, Rgb textHoverColor, Rgb textDisabledColor)
    {
        this.text = text;

        fState = isChecked ?? false ? Native.MFS_CHECKED : 0;
        fState = isDisabled ? Native.MFS_DISABLED : 0;

        this.backgroundColor = backgroundColor;
        this.backgroundHoverColor = backgroundHoverColor;
        this.backgroundDisabledColor = backgroundDisabledColor;
        this.textColor = textColor;
        this.textHoverColor = textHoverColor;
        this.textDisabledColor = textDisabledColor;
    }

    private protected void Update() => Changed?.Invoke();
}