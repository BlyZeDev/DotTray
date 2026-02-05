namespace DotTray;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using System;

/// <summary>
/// Represents a <see cref="NotifyIcon"/> menu item
/// </summary>
public sealed class MenuItem : MenuItemBase, IClickable
{
    private const float HeightMultiplier = 1.6f;

    private RECTF Hitbox;

    internal bool HasSubMenu => SubMenu.Count > 0;

    private string DrawableText => Text.Replace("\uFE0F", "");

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

    /// <inheritdoc/>
    internal protected override void Measure(PopupMenuInfo info, out float width, out float height)
    {
        height = HeightMultiplier * info.AbsoluteFontSize;
        var layoutRect = new RECTF
        {
            X = 0,
            Y = 0,
            Width = float.MaxValue,
            Height = height
        };

        _ = PInvoke.GdipMeasureString(info.GdiGraphicsHandle, DrawableText, DrawableText.Length, info.FontFamilyHandle, ref layoutRect, info.FontFormatHandle, out var boundingBox, out _, out _);
        width = boundingBox.Width;
    }

    /// <inheritdoc/>
    internal protected override void Draw(PopupMenuInfo info)
    {
        
    }

    public bool IsHovered(int x, int y) => throw new NotImplementedException();

    public void OnClick() => throw new NotImplementedException();
}