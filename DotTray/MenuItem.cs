namespace DotTray;

using DotTray.Internal.Win32;
using System;

/// <summary>
/// Represents a <see cref="NotifyIcon"/> menu item
/// </summary>
public sealed class MenuItem : MenuItemBase
{
    internal override float Height => 40f;

    internal RECTF HitBox { get; set; }
    internal bool HasSubMenu => SubMenu.Count > 0;

    /// <summary>
    /// The displayed text
    /// </summary>
    public string Text
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
    /// <see langword="true"/> if this <see cref="MenuItem"/> is checked, otherwise <see langword="false"/>.<br/>
    /// <see langword="null"/> if this <see cref="MenuItem"/> is not checkable
    /// </summary>
    public bool? IsChecked
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
    /// <see langword="true"/> if this <see cref="MenuItem"/> is disabled, otherwise <see langword="false"/>
    /// </summary>
    public bool IsDisabled
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
    /// The default background color
    /// </summary>
    public TrayColor BackgroundColor
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
    /// The background color if this item is hovered over
    /// </summary>
    public TrayColor BackgroundHoverColor
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
    /// The background color if this item is disabled
    /// </summary>
    public TrayColor BackgroundDisabledColor
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
    /// The default text color
    /// </summary>
    public TrayColor TextColor
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
    /// The text color if this item is hovered over
    /// </summary>
    public TrayColor TextHoverColor
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
    /// The text color if this item is disabled
    /// </summary>
    public TrayColor TextDisabledColor
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
    /// The <see cref="Action{MenuItemInteractionArgs}"/> to invoke if this <see cref="MenuItem"/> is clicked
    /// </summary>
    public Action<MenuItemClickedArgs>? Clicked { get; set; }

    /// <summary>
    /// The submenu for this <see cref="MenuItem"/>
    /// </summary>
    public MenuItemCollection SubMenu { get; }

    internal MenuItem(string text, Action<MenuItem> defaultMenuItemConfig, Action<SeparatorItem> defaultSeparatorItemConfig)
    {
        Text = text;
        SubMenu = new MenuItemCollection(defaultMenuItemConfig, defaultSeparatorItemConfig);
    }
}