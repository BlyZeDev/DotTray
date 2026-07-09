namespace DotTray.Popup.Default;

using DotTray.Abstract;
using System;
using System.Drawing;

/// <summary>
/// The default popup behaviour handler
/// </summary>
public sealed class DefaultPopupMenuHandler : PopupMenuHandler
{
    /// <summary>
    /// The <see cref="MenuItemCollection"/> of this <see cref="DefaultPopupMenuHandler"/> instance
    /// </summary>
    public MenuItemCollection MenuItems { get; }

    /// <summary>
    /// The background color of this <see cref="DefaultPopupMenuHandler"/> instance
    /// </summary>
    public TrayColor Color { get; private set; }

    internal DefaultPopupMenuHandler()
    {
        MenuItems = [];
        Color = TrayColor.Black;
    }

    /// <summary>
    /// Sets the <see cref="Color"/> of this <see cref="DefaultPopupMenuHandler"/> instance
    /// </summary>
    /// <param name="color">The color to set for <see cref="Color"/></param>
    public void SetColor(TrayColor color)
    {
        if (Color == color) return;

        Color = color;
    }

    /// <inheritdoc/>
    protected override void Show<THandler>(NotifyIcon<THandler> owner, Point mousePosition) => ShowRoot(owner);

    /// <inheritdoc/>
    protected override void ShowContext<THandler>(NotifyIcon<THandler> owner, Point mousePosition) => ShowRoot(owner);

    private void ShowRoot<THandler>(NotifyIcon<THandler> owner) where THandler : class, INotifyIconHandler
    {
        if (MenuItems.IsEmpty) return;

        var nativeOwner = owner as NotifyIcon<DefaultPopupMenuHandler>
            ?? throw new InvalidOperationException($"Expected owner to be of type {typeof(NotifyIcon<DefaultPopupMenuHandler>)}");

        var tree = PopupMenuTree.Show(nativeOwner, true);
    }
}