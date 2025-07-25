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
    /// The <see cref="Action{T1, T2}"/> to invoke if this <see cref="MenuItem"/> is clicked
    /// </summary>
    public Action<MenuItem, NotifyIcon>? Click { get; set; }

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
    }

    /// <summary>
    /// Creates a deep copy of this <see cref="MenuItem"/> instance
    /// </summary>
    /// <returns><see cref="MenuItem"/></returns>
    public MenuItem Copy() => new MenuItem(this);

    internal event Action? Changed;
}