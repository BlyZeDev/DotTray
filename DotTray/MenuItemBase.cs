namespace DotTray;

using System;

/// <summary>
/// Represents the base for a <see cref="NotifyIcon"/> menu item
/// </summary>
public abstract class MenuItemBase
{
    internal event Action? Updated;

    /// <summary>
    /// Calling this method will trigger a redraw of the menu containing this <see cref="MenuItemBase"/>
    /// </summary>
    protected void Update() => Updated?.Invoke();

    internal protected abstract void Measure(PopupMenuInfo info, out float width, out float height);

    internal protected abstract void Draw(PopupMenuInfo info);
}