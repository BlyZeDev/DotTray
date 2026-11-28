namespace DotTray.Direct;

using DotTray.Direct.Coloring;
using DotTray.Direct.Internal.Popup;
using DotTray.Direct.Primitives;

/// <summary>
/// The default popup behaviour handler, rendering with Direct2D and DirectWrite
/// </summary>
/// <remarks>
/// One instance can be shared by multiple <see cref="NotifyIcon{THandler}"/> instances, only one popup is open at a time.<br/>
/// The popups run on a dedicated background thread that is owned by this instance, so <see cref="ItemInteractedEventArgs"/> handlers are invoked on that thread
/// </remarks>
public class DirectPopupMenuHandler : PopupMenuHandler, IDisposable
{
    private readonly PopupThread _thread;

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
            _thread.ForceUpdate();
        }
    }

    /// <summary>
    /// Whether a popup of this instance is currently open
    /// </summary>
    public bool IsOpen => _thread.IsOpen;

    /// <summary>
    /// Initializes a new <see cref="DirectPopupMenuHandler"/> instance
    /// </summary>
    /// <remarks>
    /// This starts the popup thread and creates the rendering device, so the first popup opens without delay
    /// </remarks>
    public DirectPopupMenuHandler()
    {
        MenuItems = [];
        _thread = new PopupThread(this);
    }

    /// <summary>
    /// Opens the popup menu with its bottom left corner at the given screen position, replacing an already open popup immediately
    /// </summary>
    /// <remarks>
    /// This can be called from any thread and returns immediately.<br/>
    /// Like every native popup menu it needs the permission of Windows to take the focus, which is given when the call is a result of user input like a click or a hotkey.
    /// Without it the popup is not shown, because it couldn't be dismissed
    /// </remarks>
    /// <param name="anchor">The screen position in pixels</param>
    public void Open(Pos anchor) => _thread.Open(anchor, nint.Zero);

    /// <summary>
    /// Closes the open popup
    /// </summary>
    /// <remarks>
    /// This can be called from any thread and returns immediately
    /// </remarks>
    public void Close() => _thread.Close();

    /// <inheritdoc/>
    /// <remarks>
    /// By default this opens the popup menu at the position of the mouse.<br/>
    /// Like native tray menus the popup hands the foreground back to the window of the notify icon when it closes, so Windows also closes the hidden icons flyout.<br/>
    /// If you don't want this behaviour override <see cref="Show{THandler}(NotifyIcon{THandler}, MousePosition)"/> and don't call the <see langword="base"/> implementation
    /// </remarks>
    protected override void Show<THandler>(NotifyIcon<THandler> owner, MousePosition mousePosition) => _thread.Open(new Pos(mousePosition.X, mousePosition.Y), owner.NativeWindowHandle);

    /// <inheritdoc/>
    /// <remarks>
    /// By default this opens the popup menu at the position of the mouse.<br/>
    /// Like native tray menus the popup hands the foreground back to the window of the notify icon when it closes, so Windows also closes the hidden icons flyout.<br/>
    /// If you don't want this behaviour override <see cref="ShowContext{THandler}(NotifyIcon{THandler}, MousePosition)"/> and don't call the <see langword="base"/> implementation
    /// </remarks>
    protected override void ShowContext<THandler>(NotifyIcon<THandler> owner, MousePosition mousePosition) => _thread.Open(new Pos(mousePosition.X, mousePosition.Y), owner.NativeWindowHandle);

    /// <summary>
    /// Closes the open popup, stops the popup thread and releases the rendering device
    /// </summary>
    /// <remarks>
    /// This can be called from any thread
    /// </remarks>
    public void Dispose()
    {
        _thread.Dispose();

        GC.SuppressFinalize(this);
    }
}