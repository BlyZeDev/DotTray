namespace DotTray.Direct;

public sealed class DirectPopupMenuHandler : PopupMenuHandler
{
    protected override void Show<THandler>(NotifyIcon<THandler> owner, MousePosition mousePosition)
    {
        throw new NotImplementedException();
    }

    protected override void ShowContext<THandler>(NotifyIcon<THandler> owner, MousePosition mousePosition)
    {
        throw new NotImplementedException();
    }
}