namespace DotTray.Drawing;

using DotTray;
using DotTray.Primitives;
using System;
using System.Drawing;

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
    /// The background brush of this <see cref="DrawingPopupMenuHandler"/> instance
    /// </summary>
    public Brush Brush { get; private set; }

    internal DrawingPopupMenuHandler()
    {
        MenuItems = [];
        Brush = SystemBrushes.Menu;
    }

    /// <summary>
    /// Sets the <see cref="Brush"/> of this <see cref="DrawingPopupMenuHandler"/> instance
    /// </summary>
    /// <param name="brush">The brush to set for <see cref="Brush"/></param>
    public void SetBrush<TBrush>(TBrush brush) where TBrush : notnull, Brush
    {
        if (Brush.Equals(brush)) return;

        Brush = brush;
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