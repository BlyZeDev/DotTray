namespace DotTray;

using DotTray.Internal;
using System;

/// <summary>
/// Represents a <see cref="NotifyIcon"/> menu item
/// </summary>
public sealed class MenuItem : IMenuItem
{
    private string text;
    private bool? isChecked;
    private bool isDisabled;
    private TrayColor backgroundColor;
    private TrayColor backgroundHoverColor;
    private TrayColor backgroundDisabledColor;
    private TrayColor textColor;
    private TrayColor textHoverColor;
    private TrayColor textDisabledColor;

    private event Action? updated;
    event Action? IMenuItem.Updated
    {
        add => updated += value;
        remove => updated -= value;
    }
    internal bool HasSubMenu => SubMenu.Count > 0;

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
    /// <see langword="true"/> if this <see cref="MenuItem"/> is checked, otherwise <see langword="false"/>
    /// <see langword="null"/> if this <see cref="MenuItem"/> is not checkable
    /// </summary>
    public bool? IsChecked
    {
        get => isChecked;
        set
        {
            if (isChecked == value) return;

            isChecked = value;
            Update();
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
            Update();
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
            Update();
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
            Update();
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
            Update();
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
            Update();
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
            Update();
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

    internal MenuItem(string text)
    {
        this.text = text;
        isChecked = null;
        isDisabled = false;
        backgroundColor = DefaultColors.MenuItemBackgroundColor;
        backgroundHoverColor = DefaultColors.MenuItemBackgroundHoverColor;
        backgroundDisabledColor = DefaultColors.MenuItemBackgroundDisabledColor;
        textColor = DefaultColors.MenuItemTextColor;
        textHoverColor = DefaultColors.MenuItemTextHoverColor;
        textDisabledColor = DefaultColors.MenuItemTextDisabledColor;
        SubMenu = [];
    }

    private void Update() => updated?.Invoke();
}