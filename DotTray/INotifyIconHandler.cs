namespace DotTray;

/// <summary>
/// Defines a contract for handling a popup menu for a <see cref="NotifyIcon"/>.
/// </summary>
public interface INotifyIconHandler
{
    /// <summary>
    /// Invoked every time an interaction with a <see cref="NotifyIcon"/> instance happens
    /// </summary>
    /// <remarks>
    /// This is invoked the same as <see cref="NotifyIcon.Interacted"/>
    /// </remarks>
    /// <param name="owner">The <see cref="NotifyIcon"/> that caused the interaction</param>
    /// <param name="args">The interaction arguments</param>
    void HandleInteraction(NotifyIcon owner, NotifyIconInteractedEventArgs args);
}