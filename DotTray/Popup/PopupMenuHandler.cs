namespace DotTray.Popup;

using System.Drawing;

/// <summary>
/// A specialized handler designed for managing standard contextual popup menus.
/// </summary>
public abstract class PopupMenuHandler : INotifyIconHandler
{
    /// <inheritdoc/>
    public void HandleInteraction(NotifyIcon owner, NotifyIconInteractedEventArgs args)
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
    /// <param name="owner">The <see cref="NotifyIcon"/> that owns this handler</param>
    /// <param name="mousePosition">The coordinates of the cursor (in screen coordinates) at the exact moment the interaction occurred</param>
    public abstract void Show(NotifyIcon owner, Point mousePosition);

    /// <summary>
    /// Called when the popup context menu is requested
    /// </summary>
    /// <param name="owner">The <see cref="NotifyIcon"/> that owns this handler</param>
    /// <param name="mousePosition">The coordinates of the cursor (in screen coordinates) at the exact moment the interaction occurred</param>
    public abstract void ShowContext(NotifyIcon owner, Point mousePosition);

    /// <summary>
    /// Called when showing the tooltip is requested
    /// </summary>
    /// <remarks>
    /// It is recommended to disable the default tooltip as showing both might clash
    /// </remarks>
    /// <param name="owner">The <see cref="NotifyIcon"/> that owns this handler</param>
    /// <param name="mousePosition">The coordinates of the cursor (in screen coordinates) at the exact moment the interaction occurred</param>
    public virtual void ShowToolTip(NotifyIcon owner, Point mousePosition) { }

    /// <summary>
    /// Called when hiding the tooltip is requested
    /// </summary>
    /// <remarks>
    /// It is recommended to disable the default tooltip as showing both might clash
    /// </remarks>
    /// <param name="owner">The <see cref="NotifyIcon"/> that owns this handler</param>
    /// <param name="mousePosition">The coordinates of the cursor (in screen coordinates) at the exact moment the interaction occurred</param>
    public virtual void HideToolTip(NotifyIcon owner, Point mousePosition) { }
}