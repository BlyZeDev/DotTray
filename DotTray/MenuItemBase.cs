namespace DotTray;

using System;

/// <summary>
/// Represents the base for a <see cref="NotifyIcon"/> menu item
/// </summary>
public abstract class MenuItemBase
{
    internal static readonly TrayColor DefaultMenuItemBackgroundColor = TrayColor.Transparent;
    internal static readonly TrayColor DefaultMenuItemTextColor = TrayColor.White;

    internal abstract float Height { get; }

    internal event Action? Updated;

    private protected void Update() => Updated?.Invoke();
}