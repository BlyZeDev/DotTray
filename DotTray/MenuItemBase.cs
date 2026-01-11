namespace DotTray;

using System;

/// <summary>
/// Represents the base for a <see cref="NotifyIcon"/> menu item
/// </summary>
public abstract class MenuItemBase
{
    internal abstract float HeightMultiplier { get; }

    internal event Action? Updated;

    private protected void Update() => Updated?.Invoke();
}