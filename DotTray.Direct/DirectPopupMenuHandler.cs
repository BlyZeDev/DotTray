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
public class DirectPopupMenuHandler : PopupMenuHandler, IDisposable
{
    private readonly Lock _lock;
    private readonly Dictionary<object, PopupMenuTree> _activeTrees;
    private readonly ThreadLocal<DirectDevice?> _devices;

    private bool disposed;

    /// <summary>
    /// The <see cref="MenuItemCollection"/> of this instance
    /// </summary>
    public MenuItemCollection MenuItems { get; }

    /// <summary>
    /// The background color of the popup windows, or <see langword="null"/> to inherit from <see cref="DirectDefaultStyles.PopupWindowBackground"/>
    /// </summary>
    public IDirectBrush? Background
    {
        get;
        set
        {
            if (EqualityComparer<IDirectBrush?>.Default.Equals(field, value)) return;

            field = value;
            MenuItems.ForceUpdate();
        }
    }

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
    /// <remarks>
    /// By default this opens the popup menu.<br/>
    /// If you don't want this behaviour override <see cref="Show{THandler}(NotifyIcon{THandler}, MousePosition)"/> and don't call the <see langword="base"/> implementation
    /// </remarks>
    protected override void Show<THandler>(NotifyIcon<THandler> owner, MousePosition mousePosition) => ShowRoot(owner);

    /// <inheritdoc/>
    /// <remarks>
    /// By default this opens the popup menu.<br/>
    /// If you don't want this behaviour override <see cref="ShowContext{THandler}(NotifyIcon{THandler}, MousePosition)"/> and don't call the <see langword="base"/> implementation
    /// </remarks>
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

        GC.SuppressFinalize(this);
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