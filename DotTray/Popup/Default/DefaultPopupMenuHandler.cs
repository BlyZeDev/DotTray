namespace DotTray.Popup.Default;

using DotTray.Abstract;
using DotTray.Popup.Default.Coloring;
using DotTray.Primitives;
using System;

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
    /// <remarks>
    /// Transparency is not supported
    /// </remarks>
    public IColorable Color { get; private set; }

    internal DefaultPopupMenuHandler()
    {
        MenuItems = [];
        Color = SolidColor.White;
    }

    /// <summary>
    /// Sets the <see cref="Color"/> of this <see cref="DefaultPopupMenuHandler"/> instance
    /// </summary>
    /// <param name="color">The color to set for <see cref="Color"/></param>
    public void SetColor<TColor>(TColor color) where TColor : notnull, IColorable
    {
        if (Color.Equals(color)) return;

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