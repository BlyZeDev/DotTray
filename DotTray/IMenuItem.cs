namespace DotTray;

using System;

/// <summary>
/// Represents the base for a <see cref="NotifyIcon"/> menu item
/// </summary>
public interface IMenuItem
{
    internal event Action? Updated;
}