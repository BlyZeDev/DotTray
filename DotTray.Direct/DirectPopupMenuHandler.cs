namespace DotTray.Direct;

using DotTray;

/// <summary>
/// The default popup behaviour handler, rendering with Direct2D and DirectWrite
/// </summary>
public sealed class DirectPopupMenuHandler : PopupMenuHandler, IDisposable
{
    private readonly Lock _lock;
    private readonly Dictionary<NotifyIcon<DirectPopupMenuHandler>, PopupMenuTree> _activeTrees;

    private volatile bool disposed;

    /// <summary>
    /// The style shared by every popup shown by this instance
    /// </summary>
    public PopupMenuStyle Style { get; }

    /// <summary>
    /// The <see cref="MenuItemCollection"/> of this instance
    /// </summary>
    public MenuItemCollection MenuItems { get; }

    /// <summary>
    /// Initializes a new <see cref="DirectPopupMenuHandler"/> instance
    /// </summary>
    public DirectPopupMenuHandler(PopupMenuStyle? style = null)
    {
        _lock = new Lock();
        _activeTrees = new Dictionary<NotifyIcon<DirectPopupMenuHandler>, PopupMenuTree>(ReferenceEqualityComparer.Instance);

        Style = style ?? new PopupMenuStyle();
        MenuItems = [];
    }

    /// <inheritdoc/>
    protected override void Show<THandler>(NotifyIcon<THandler> owner, MousePosition mousePosition)
    {
        var actualOwner = owner as NotifyIcon<DirectPopupMenuHandler>;
        ShowRoot(actualOwner!);
    }

    /// <inheritdoc/>
    protected override void ShowContext<THandler>(NotifyIcon<THandler> owner, MousePosition mousePosition)
    {
        var actualOwner = owner as NotifyIcon<DirectPopupMenuHandler>;
        ShowRoot(actualOwner!);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;

        PopupMenuTree[] trees;
        lock (_lock)
        {
            trees = _activeTrees.Values.ToArray();
        }

        foreach (var tree in trees)
        {
            tree.Dispose();
        }
    }

    private void ShowRoot(NotifyIcon<DirectPopupMenuHandler> owner)
    {
        if (disposed || MenuItems.IsEmpty) return;

        PopupMenuTree? active;
        lock (_lock)
        {
            _activeTrees.TryGetValue(owner, out active);
        }

        var tree = active is not null ? active.Regrow(true) : PopupMenuTree.Show(this, true);

        tree.Disposed += () =>
        {
            lock (_lock)
            {
                if (_activeTrees.TryGetValue(owner, out var current) && ReferenceEquals(current, tree)) _activeTrees.Remove(owner);
            }
        };

        lock (_lock)
        {
            _activeTrees[owner] = tree;
        }
    }
}