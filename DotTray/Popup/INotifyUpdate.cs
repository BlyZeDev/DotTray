namespace DotTray.Popup;

using System;

/// <summary>
/// Defines a contract for update notifications
/// </summary>
public interface INotifyUpdate
{
    /// <summary>
    /// Fired on update
    /// </summary>
    event Action? Updated;
}