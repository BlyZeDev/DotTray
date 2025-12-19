namespace DotTray;

using System;

/// <summary>
/// Represents the base for a <see cref="NotifyIcon"/> menu item
/// </summary>
public abstract class MenuItemBase
{
    internal static readonly TrayColor DefaultMenuItemBackgroundColor = TrayColor.Transparent;
    internal static readonly TrayColor DefaultMenuItemBackgroundHoverColor = new TrayColor(0, 120, 215);
    internal static readonly TrayColor DefaultMenuItemBackgroundDisabledColor = TrayColor.Gray;
    internal static readonly TrayColor DefaultMenuItemTextColor = TrayColor.White;
    internal static readonly TrayColor DefaultMenuItemTextHoverColor = DefaultMenuItemTextColor;
    internal static readonly TrayColor DefaultMenuItemTextDisabledColor = new TrayColor(109, 109, 109);

    internal static readonly TrayColor DefaultSeparatorBackgroundColor = DefaultMenuItemBackgroundColor;
    internal static readonly TrayColor DefaultSeparatorLineColor = DefaultMenuItemTextColor;

    internal abstract float Height { get; }

    internal event Action? Updated;

    private protected void Update() => Updated?.Invoke();
}