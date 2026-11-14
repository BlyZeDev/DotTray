namespace DotTray.Direct;

using DotTray;
using DotTray.Direct.Coloring;
using DotTray.Direct.Internal;
using DotTray.Direct.Internal.Rendering;

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
    private readonly ThreadLocal<DirectDevice?> _devices;

    private bool disposed;

    /// <summary>
    /// The background color of the popup windows
    /// </summary>
    public IDirectBrush Background { get; set; } = DirectDefaultStyles.PopupWindowBackground;

    /// <summary>
    /// The <see cref="MenuItemCollection"/> of this instance
    /// </summary>
    public MenuItemCollection MenuItems { get; }

    /// <summary>
    /// Initializes a new <see cref="DirectPopupMenuHandler"/> instance
    /// </summary>
    public DirectPopupMenuHandler()
    {
        _lock = new Lock();
        _activeTrees = new Dictionary<object, PopupMenuTree>(ReferenceEqualityComparer.Instance);
        _devices = new ThreadLocal<DirectDevice?>(true);

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
        DirectDevice?[] devices;

        lock (_lock)
        {
            if (disposed) return;
            disposed = true;

            trees = [.. _activeTrees.Values];
            _activeTrees.Clear();

            devices = [.. _devices.Values];
            _devices.Dispose();
        }

        foreach (var tree in trees) tree.Dispose();
        foreach (var device in devices) device?.Dispose();
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

        var device = AcquireDevice();
        if (device is null) return;

        PopupMenuTree tree;
        try
        {
            tree = new PopupMenuTree(this, device, MenuItems);
        }
        catch
        {
            device.Dispose();
            throw;
        }

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

    private DirectDevice? AcquireDevice()
    {
        lock (_lock)
        {
            if (disposed) return null;

            var device = _devices.Value;

            if (device is null || device.IsLost)
            {
                if (device is not null)
                {
                    _devices.Value = null;
                    device.Dispose();
                }

                device = new DirectDevice();
                _devices.Value = device;
            }

            return device.Retain();
        }
    }


    private void Untrack(object owner, PopupMenuTree tree)
    {
        lock (_lock)
        {
            if (_activeTrees.TryGetValue(owner, out var current) && ReferenceEquals(current, tree)) _activeTrees.Remove(owner);
        }
    }
}