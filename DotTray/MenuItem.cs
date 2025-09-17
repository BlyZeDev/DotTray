namespace DotTray;

using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a <see cref="NotifyIcon"/> menu item
/// </summary>
public sealed record MenuItem : IMenuItem
{
    private string text;
    private bool? isChecked;
    private bool isDisabled;
    private Rgb backgroundColor = new Rgb(30, 30, 30);
    private Rgb backgroundHoverColor = new Rgb(50, 120, 220);
    private Rgb backgroundDisabledColor = new Rgb(100, 100, 100);
    private Rgb textColor = new Rgb(240, 240, 240);
    private Rgb textHoverColor = new Rgb(255, 255, 255);
    private Rgb textDisabledColor = new Rgb(30, 30, 30);

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
            Changed?.Invoke();
        }
    }

    /// <summary>
    /// <see langword="true"/> if this <see cref="MenuItem"/> is checked, otherwise <see langword="false"/>.
    /// <see langword="null"/> if this <see cref="MenuItem"/> is not checkable.<br/>
    /// </summary>
    public bool? IsChecked
    {
        get => isChecked;
        set
        {
            if (isChecked == value) return;

            isChecked = value;
            Changed?.Invoke();
        }
    }

    /// <summary>
    /// <see langword="true"/> if this <see cref="MenuItem"/> is disabled, otherwise <see langword="false"/>
    /// </summary>
    public bool IsDisabled
    {
        get => isDisabled;
        set
        {
            if (isDisabled == value) return;

            isDisabled = value;
            Changed?.Invoke();
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
            Changed?.Invoke();
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
            Changed?.Invoke();
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
            Changed?.Invoke();
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
            Changed?.Invoke();
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
            Changed?.Invoke();
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
            Changed?.Invoke();
        }
    }

    /// <summary>
    /// The <see cref="Action{MenuItemClickedArgs}"/> to invoke if this <see cref="MenuItem"/> is clicked
    /// </summary>
    public Action<MenuItemClickedArgs>? Click { get; set; }

    /// <summary>
    /// The sub menu items of this <see cref="MenuItem"/>
    /// </summary>
    public MenuItemCollection SubMenu { get; init; } = [];

    [SetsRequiredMembers]
    private MenuItem(MenuItem instance)
    {
        Text = instance.Text;
        IsChecked = instance.IsChecked;
        IsDisabled = instance.IsDisabled;
        Click = instance.Click;
        SubMenu = instance.SubMenu.Copy();
        BackgroundColor = instance.BackgroundColor;
        BackgroundHoverColor = instance.BackgroundHoverColor;
        BackgroundDisabledColor = instance.BackgroundDisabledColor;
        TextColor = instance.TextColor;
        TextHoverColor = instance.TextHoverColor;
        TextDisabledColor = instance.TextDisabledColor;
    }

    /// <summary>
    /// Creates a deep copy of this <see cref="MenuItem"/> instance
    /// </summary>
    /// <returns><see cref="MenuItem"/></returns>
    public MenuItem Copy() => new MenuItem(this);

    internal event Action? Changed;
}