namespace DotTray.Direct;

using DotTray;

/// <summary>
/// The default popup behaviour handler, rendering with Direct2D and DirectWrite
/// </summary>
/// <remarks>
/// One instance can be shared by multiple <see cref="NotifyIcon{THandler}"/> instances at the same time, every icon gets its own popup tree
/// </remarks>
public sealed class DirectPopupMenuHandler : PopupMenuHandler, IDisposable
{
    private readonly Lock _lock;
    private readonly Dictionary<object, PopupMenuTree> _activeTrees;

    private bool disposed;

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
    /// <param name="style">The style shared by every popup shown by this instance, or <see langword="null"/> for the default style</param>
    public DirectPopupMenuHandler(PopupMenuStyle? style = null)
    {
        _lock = new Lock();
        _activeTrees = new Dictionary<object, PopupMenuTree>(ReferenceEqualityComparer.Instance);

        Style = style ?? new PopupMenuStyle();
        MenuItems = [];
    }

    /// <inheritdoc/>
    protected override void Show<THandler>(NotifyIcon<THandler> owner, MousePosition mousePosition) => ShowRoot(owner);

    /// <inheritdoc/>
    protected override void ShowContext<THandler>(NotifyIcon<THandler> owner, MousePosition mousePosition) => ShowRoot(owner);

    /// <summary>
    /// Closes every popup tree of this instance and stops it from showing new ones
    /// </summary>
    /// <remarks>
    /// This can be called from any thread
    /// </remarks>
    public void Dispose()
    {
        PopupMenuTree[] trees;

        lock (_lock)
        {
            if (disposed) return;
            disposed = true;

            trees = [.. _activeTrees.Values];
            _activeTrees.Clear();
        }

        foreach (var tree in trees) tree.Dispose();
    }

    private void ShowRoot(object owner)
    {
        if (MenuItems.IsEmpty) return;

        PopupMenuTree? active;

        lock (_lock)
        {
            if (disposed) return;

            _activeTrees.TryGetValue(owner, out active);
        }

        active?.Dispose();

        var tree = PopupMenuTree.Show(this, true);

        tree.Disposed += () => Untrack(owner, tree);

        lock (_lock)
        {
            if (!disposed)
            {
                _activeTrees[owner] = tree;
                return;
            }
        }

        tree.Dispose();
    }

    private void Untrack(object owner, PopupMenuTree tree)
    {
        lock (_lock)
        {
            if (_activeTrees.TryGetValue(owner, out var current) && ReferenceEquals(current, tree)) _activeTrees.Remove(owner);
        }
    }
}