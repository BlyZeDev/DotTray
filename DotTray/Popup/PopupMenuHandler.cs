namespace DotTray.Popup;

using DotTray.Abstract;
using DotTray.Primitives;

/// <summary>
/// A specialized handler designed for managing standard contextual popup menus.
/// </summary>
public abstract class PopupMenuHandler : INotifyIconHandler
{
    /// <inheritdoc/>
    void INotifyIconHandler.HandleInteraction<THandler>(NotifyIcon<THandler> owner, NotifyIconInteractedEventArgs args)
    {
        switch (args.Type)
        {
            case InteractionType.Select: Show(owner, args.MousePosition); break;
            case InteractionType.ContextMenu: ShowContext(owner, args.MousePosition); break;
            case InteractionType.PopupOpen: ShowToolTip(owner, args.MousePosition); break;
            case InteractionType.PopupClose: HideToolTip(owner, args.MousePosition); break;
        }
    }

    /// <summary>
    /// Called when the popup menu is requested
    /// </summary>
    /// <param name="owner">The <see cref="NotifyIcon{THandler}"/> that owns this handler</param>
    /// <param name="mousePosition">The coordinates of the cursor (in screen coordinates) at the exact moment the interaction occurred</param>
    protected abstract void Show<THandler>(NotifyIcon<THandler> owner, Point mousePosition) where THandler : class, INotifyIconHandler;

    /// <summary>
    /// Called when the popup context menu is requested
    /// </summary>
    /// <param name="owner">The <see cref="NotifyIcon{THandler}"/> that owns this handler</param>
    /// <param name="mousePosition">The coordinates of the cursor (in screen coordinates) at the exact moment the interaction occurred</param>
    protected abstract void ShowContext<THandler>(NotifyIcon<THandler> owner, Point mousePosition) where THandler : class, INotifyIconHandler;

    /// <summary>
    /// Called when showing the tooltip is requested
    /// </summary>
    /// <remarks>
    /// It is recommended to disable the default tooltip as showing both might clash
    /// </remarks>
    /// <param name="owner">The <see cref="NotifyIcon{THandler}"/> that owns this handler</param>
    /// <param name="mousePosition">The coordinates of the cursor (in screen coordinates) at the exact moment the interaction occurred</param>
    protected virtual void ShowToolTip<THandler>(NotifyIcon<THandler> owner, Point mousePosition) where THandler : class, INotifyIconHandler { }

    /// <summary>
    /// Called when hiding the tooltip is requested
    /// </summary>
    /// <remarks>
    /// It is recommended to disable the default tooltip as showing both might clash
    /// </remarks>
    /// <param name="owner">The <see cref="NotifyIcon{THandler}"/> that owns this handler</param>
    /// <param name="mousePosition">The coordinates of the cursor (in screen coordinates) at the exact moment the interaction occurred</param>
    protected virtual void HideToolTip<THandler>(NotifyIcon<THandler> owner, Point mousePosition) where THandler : class, INotifyIconHandler { }
}