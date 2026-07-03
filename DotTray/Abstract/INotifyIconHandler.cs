namespace DotTray.Abstract;

/// <summary>
/// Defines a contract for handling a popup menu for a <see cref="NotifyIcon{THandler}"/>
/// </summary>
public interface INotifyIconHandler
{
    /// <summary>
    /// Invoked every time an interaction with a <see cref="NotifyIcon{THandler}"/> instance happens
    /// </summary>
    /// <remarks>
    /// This is invoked the same as <see cref="NotifyIcon{THandler}.Interacted"/>
    /// </remarks>
    /// <param name="owner">The <see cref="NotifyIcon{THandler}"/> that caused the interaction</param>
    /// <param name="args">The interaction arguments</param>
    void HandleInteraction<THandler>(NotifyIcon<THandler> owner, NotifyIconInteractedEventArgs args) where THandler : class, INotifyIconHandler;
}