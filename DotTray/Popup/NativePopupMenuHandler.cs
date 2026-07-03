namespace DotTray.Popup;

using DotTray.Abstract;
using System.Drawing;

/// <summary>
/// The default popup behaviour handler
/// </summary>
public sealed class NativePopupMenuHandler : PopupMenuHandler
{
    /// <inheritdoc/>
    protected override void Show<THandler>(NotifyIcon<THandler> owner, Point mousePosition) => ShowRoot(owner, mousePosition);

    /// <inheritdoc/>
    protected override void ShowContext<THandler>(NotifyIcon<THandler> owner, Point mousePosition) => ShowRoot(owner, mousePosition);

    private static void ShowRoot<THandler>(NotifyIcon<THandler> owner, Point mousePosition) where THandler : class, INotifyIconHandler
    {
        //Show root menu
    }
}