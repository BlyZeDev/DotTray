namespace DotTray.Drawing;

using DotTray;
using DotTray.Drawing.Coloring;
using DotTray.Primitives;
using System;

/// <summary>
/// The default popup behaviour handler
/// </summary>
public sealed class DrawingPopupMenuHandler : PopupMenuHandler
{
    private PopupMenuTree? _activeTree;

    /// <summary>
    /// The <see cref="MenuItemCollection"/> of this <see cref="DrawingPopupMenuHandler"/> instance
    /// </summary>
    public MenuItemCollection MenuItems { get; }

    /// <summary>
    /// The background color of this <see cref="DrawingPopupMenuHandler"/> instance
    /// </summary>
    /// <remarks>
    /// Transparency is not supported
    /// </remarks>
    public IColorable Color { get; private set; }

    internal DrawingPopupMenuHandler()
    {
        MenuItems = [];
        Color = SolidColor.White;
    }

    /// <summary>
    /// Sets the <see cref="Color"/> of this <see cref="DrawingPopupMenuHandler"/> instance
    /// </summary>
    /// <param name="color">The color to set for <see cref="Color"/></param>
    public void SetColor<TColor>(TColor color) where TColor : notnull, IColorable
    {
        if (Color.Equals(color)) return;

        Color = color;
    }

    /// <inheritdoc/>
    protected override void Show<THandler>(NotifyIcon<THandler> owner, Pos mousePosition) => ShowRoot(owner);

    /// <inheritdoc/>
    protected override void ShowContext<THandler>(NotifyIcon<THandler> owner, Pos mousePosition) => ShowRoot(owner);

    private void ShowRoot<THandler>(NotifyIcon<THandler> owner) where THandler : class, INotifyIconHandler
    {
        if (MenuItems.IsEmpty) return;

        var nativeOwner = owner as NotifyIcon<DrawingPopupMenuHandler>
            ?? throw new InvalidOperationException($"Expected owner to be of type {typeof(NotifyIcon<DrawingPopupMenuHandler>)}");

        var tree = _activeTree is not null ? _activeTree.Regrow(true) : PopupMenuTree.Show(nativeOwner, true);

        tree.Disposed += () => _activeTree = null;
        _activeTree = tree;
    }
}