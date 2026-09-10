namespace DotTray;

/// <summary>
/// Represents an implementation of <see cref="INotifyIconHandler"/> that does nothing on interaction
/// </summary>
public sealed class NotifyIconNullHandler : INotifyIconHandler
{
    internal static readonly NotifyIconNullHandler Instance = new NotifyIconNullHandler();

    private NotifyIconNullHandler() { }

    void INotifyIconHandler.HandleInteraction<THandler>(NotifyIcon<THandler> owner, NotifyIconInteractedEventArgs args) { }
}