namespace DotTray.Popup;

using System.Drawing;

/// <summary>
/// The default popup behaviour handler
/// </summary>
public sealed class DefaultPopupMenuHandler : PopupMenuHandler
{
    /// <inheritdoc/>
    public override void Show(NotifyIcon owner, Point mousePosition)
    {
        if (owner.MenuItems.IsEmpty) return;

        ShowRoot(owner, mousePosition);
    }

    /// <inheritdoc/>
    public override void ShowContext(NotifyIcon owner, Point mousePosition)
    {
        if (owner.MenuItems.IsEmpty) return;

        ShowRoot(owner, mousePosition);
    }

    private static void ShowRoot(NotifyIcon owner, Point mousePosition)
    {

    }
}