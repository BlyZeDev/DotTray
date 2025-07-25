namespace DotTray;

using DotTray.Internal;
using DotTray.Internal.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Represents a Notification Icon that is displayed in the Taskbar
/// </summary>
/// <remarks>
/// Due to the fact that the Windows notification icon size is 16x16,
/// it is recommended that the icon contains a 16x16 variant.<br/>
/// If no 16x16 variant is available,
/// the quality can suffer greatly as it then has to be scaled from another existing size.
/// </remarks>
[SupportedOSPlatform("Windows")]
public sealed partial class NotifyIcon : IDisposable
{
    private const string WindowClassName = $"{nameof(DotTray)}NotifyIconWindow";

    private readonly Thread _trayLoopThread;

    private readonly Dictionary<int, Action> _menuActions;
    private readonly Dictionary<string, nint> _subMenus;

    private nint icoHandle;
    private bool needsIcoDestroy;

    private nint instanceHandle;
    private GCHandle thisHandle;
    private nint hWnd;
    private nint trayMenu;

    private BalloonNotification? nextBalloon;
    private bool menuRefreshQueued;
    private int nextCommandId;

    /// <summary>
    /// The <see cref="MenuItemCollection"/> of this <see cref="NotifyIcon"/> instance
    /// </summary>
    public MenuItemCollection MenuItems { get; }

    /// <summary>
    /// The <see cref="ToolTip"/> of this <see cref="NotifyIcon"/> instance
    /// </summary>
    public string ToolTip { get; private set; }

    private NotifyIcon(nint icoHandle, bool needsIcoDestroy, MenuItemCollection menuItems, Action onInitializationFinished, CancellationToken cancellationToken)
    {
        this.icoHandle = icoHandle;
        this.needsIcoDestroy = needsIcoDestroy;

        _menuActions = [];
        _subMenus = [];

        menuRefreshQueued = false;
        nextCommandId = 1000;

        MenuItems = menuItems;
        ToolTip = "";

        MonitorMenuItems(MenuItems);

        _trayLoopThread = new Thread(() =>
        {
            instanceHandle = Native.GetModuleHandle(null);

            var wndProc = new Native.WndProc(WndProcFunc);
            var wndClass = new WNDCLASS
            {
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProc),
                hInstance = instanceHandle,
                lpszClassName = WindowClassName
            };
            Native.RegisterClass(ref wndClass);

            hWnd = Native.CreateWindowEx(0, WindowClassName, "", 0, 0, 0, 0, 0, 0, 0, instanceHandle, 0);
            thisHandle = GCHandle.Alloc(this, GCHandleType.Normal);

            Native.SetWindowLongPtr(hWnd, Native.GWLP_USERDATA, GCHandle.ToIntPtr(thisHandle));

            onInitializationFinished();

            var iconData = new NOTIFYICONDATA
            {
                cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                hWnd = hWnd,
                uID = Native.ID_TRAY_ICON, //TODO: Allow multiple TrayIcons at once by using a unique ID for every running instance
                uFlags = Native.NIF_MESSAGE | Native.NIF_ICON,
                uCallbackMessage = Native.WM_APP_TRAYICON,
                hIcon = icoHandle
            };
            Native.Shell_NotifyIcon(Native.NIM_ADD, ref iconData);

            using (var registration = cancellationToken.Register(() => Native.PostMessage(hWnd, Native.WM_APP_TRAYICON_QUIT, 0, 0)))
            {
                Native.PostMessage(hWnd, Native.WM_APP_TRAYICON_REBUILD, 0, 0);
                Native.PostMessage(hWnd, Native.WM_APP_TRAYICON_TOOLTIP, 0, 0);

                while (Native.GetMessage(out var message, nint.Zero, 0, 0))
                {
                    if (hWnd == message.hwnd)
                    {
                        Native.TranslateMessage(ref message);
                        Native.DispatchMessage(ref message);
                    }
                }

                iconData = new NOTIFYICONDATA
                {
                    cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                    hWnd = hWnd,
                    uID = Native.ID_TRAY_ICON
                };
                Native.Shell_NotifyIcon(Native.NIM_DELETE, ref iconData);

                if (needsIcoDestroy) Native.DestroyIcon(icoHandle);

                if (trayMenu != nint.Zero)
                {
                    Native.DestroyMenu(trayMenu);
                    trayMenu = nint.Zero;
                }

                if (hWnd != nint.Zero)
                {
                    Native.SetWindowLongPtr(hWnd, Native.GWLP_USERDATA, nint.Zero);
                    if (thisHandle.IsAllocated) thisHandle.Free();

                    Native.DestroyWindow(hWnd);
                    hWnd = nint.Zero;

                    Native.UnregisterClass(WindowClassName, instanceHandle);
                    instanceHandle = nint.Zero;
                }
            }
        });
        _trayLoopThread.SetApartmentState(ApartmentState.STA);
        _trayLoopThread.Start();
    }

    /// <summary>
    /// Sets the Icon of this <see cref="NotifyIcon"/> instance
    /// </summary>
    /// <param name="icoPath">The path to a .ico file</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="FileLoadException"></exception>
    public void SetIcon(string icoPath) => SetIcon(PrepareIconHandle(icoPath), true);

    /// <summary>
    /// Sets the Icon of this <see cref="NotifyIcon"/> instance
    /// </summary>
    /// <remarks>
    /// If <paramref name="icoHandle"/> == <see cref="nint.Zero"/> the icon will be invisible.<br/>
    /// <paramref name="icoHandle"/> will not be destroyed, the responsibility lies with the caller
    /// </remarks>
    /// <param name="icoHandle">The handle of a .ico file</param>
    public void SetIcon(nint icoHandle) => SetIcon(icoHandle, false);

    /// <summary>
    /// Sets the <see cref="ToolTip"/> of this <see cref="NotifyIcon"/> instance
    /// </summary>
    /// <remarks>
    /// <paramref name="toolTip"/> is truncated to fit into the allowed Windows tooltip character length
    /// </remarks>
    /// <param name="toolTip">The text to set as <see cref="ToolTip"/></param>
    public void SetToolTip(string toolTip)
    {
        toolTip = toolTip.Length > NOTIFYICONDATA.SZTIP_LENGTH ? toolTip[..NOTIFYICONDATA.SZTIP_LENGTH] : toolTip;

        if (ToolTip.Equals(toolTip, StringComparison.Ordinal)) return;

        ToolTip = toolTip;
        Native.PostMessage(hWnd, Native.WM_APP_TRAYICON_TOOLTIP, 0, 0);
    }

    /// <summary>
    /// Shows a balloon notification
    /// </summary>
    /// <param name="balloon">The balloon notification to show</param>
    public void ShowBalloon(BalloonNotification balloon)
    {
        Interlocked.Exchange(ref nextBalloon, balloon with
        {
            Title = balloon.Title.Length > NOTIFYICONDATA.SZINFOTITLE_LENGTH ? balloon.Title[..NOTIFYICONDATA.SZINFOTITLE_LENGTH] : balloon.Title,
            Message = balloon.Message.Length > NOTIFYICONDATA.SZINFO_LENGTH ? balloon.Message[..NOTIFYICONDATA.SZINFO_LENGTH] : balloon.Message
        });

        Native.PostMessage(hWnd, Native.WM_APP_TRAYICON_BALLOON, 0, 0);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        MonitorMenuItems(MenuItems, true);
        Native.PostMessage(hWnd, Native.WM_APP_TRAYICON_QUIT, 0, 0);

        if (_trayLoopThread.IsAlive) _trayLoopThread.Join();
    }

    /// <summary>
    /// Creates and runs a <see cref="NotifyIcon"/> instance synchronously
    /// </summary>
    /// <remarks>
    /// This will block until the <see cref="NotifyIcon"/> instance is ready or an <see cref="Exception"/> occurs
    /// </remarks>
    /// <param name="icoPath">The path to a .ico file</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to stop this <see cref="NotifyIcon"/> instance</param>
    /// <returns><see cref="NotifyIcon"/></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="FileLoadException"></exception>
    public static NotifyIcon Run(string icoPath, CancellationToken cancellationToken)
        => Run(PrepareIconHandle(icoPath), true, [], cancellationToken);

    /// <summary>
    /// Creates and runs a <see cref="NotifyIcon"/> instance synchronously
    /// </summary>
    /// <remarks>
    /// This will block until the <see cref="NotifyIcon"/> instance is ready or an <see cref="Exception"/> occurs
    /// </remarks>
    /// <param name="icoPath">The path to a .ico file</param>
    /// <param name="menuItems">The initial <see cref="MenuItemCollection"/></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to stop this <see cref="NotifyIcon"/> instance</param>
    /// <returns><see cref="NotifyIcon"/></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="FileLoadException"></exception>
    public static NotifyIcon Run(string icoPath, MenuItemCollection menuItems, CancellationToken cancellationToken)
        => Run(PrepareIconHandle(icoPath), true, menuItems, cancellationToken);

    /// <summary>
    /// Creates and runs a <see cref="NotifyIcon"/> instance synchronously
    /// </summary>
    /// <remarks>
    /// This will block until the <see cref="NotifyIcon"/> instance is ready or an <see cref="Exception"/> occurs.<br/>
    /// <paramref name="icoHandle"/> will not be destroyed, the responsibility lies with the caller
    /// </remarks>
    /// <param name="icoHandle">The handle of a .ico file</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to stop this <see cref="NotifyIcon"/> instance</param>
    /// <returns><see cref="NotifyIcon"/></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static NotifyIcon Run(nint icoHandle, CancellationToken cancellationToken)
    {
        return icoHandle == nint.Zero
            ? throw new ArgumentNullException(nameof(icoHandle), "The handle cannot be null")
            : Run(icoHandle, false, [], cancellationToken);
    }

    /// <summary>
    /// Creates and runs a <see cref="NotifyIcon"/> instance synchronously
    /// </summary>
    /// <remarks>
    /// This will block until the <see cref="NotifyIcon"/> instance is ready or an <see cref="Exception"/> occurs.<br/>
    /// <paramref name="icoHandle"/> will not be destroyed, the responsibility lies with the caller
    /// </remarks>
    /// <param name="icoHandle">The handle of a .ico file</param>
    /// <param name="menuItems">The initial <see cref="MenuItemCollection"/></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to stop this <see cref="NotifyIcon"/> instance</param>
    /// <returns><see cref="NotifyIcon"/></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static NotifyIcon Run(nint icoHandle, MenuItemCollection menuItems, CancellationToken cancellationToken)
    {
        return icoHandle == nint.Zero
            ? throw new ArgumentNullException(nameof(icoHandle), "The handle cannot be null")
            : Run(icoHandle, false, menuItems, cancellationToken);
    }

    /// <summary>
    /// Creates and runs a <see cref="NotifyIcon"/> instance asynchronously
    /// </summary>
    /// <param name="icoPath">The path to a .ico file</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to stop this <see cref="NotifyIcon"/> instance</param>
    /// <returns><see cref="NotifyIcon"/></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="FileLoadException"></exception>
    public static Task<NotifyIcon> RunAsync(string icoPath, CancellationToken cancellationToken)
        => RunAsync(PrepareIconHandle(icoPath), true, [], cancellationToken);

    /// <summary>
    /// Creates and runs a <see cref="NotifyIcon"/> instance asynchronously
    /// </summary>
    /// <param name="icoPath">The path to a .ico file</param>
    /// <param name="menuItems">The initial <see cref="MenuItemCollection"/></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to stop this <see cref="NotifyIcon"/> instance</param>
    /// <returns><see cref="NotifyIcon"/></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="FileLoadException"></exception>
    public static Task<NotifyIcon> RunAsync(string icoPath, MenuItemCollection menuItems, CancellationToken cancellationToken)
        => RunAsync(PrepareIconHandle(icoPath), true, menuItems, cancellationToken);

    /// <summary>
    /// Creates and runs a <see cref="NotifyIcon"/> instance asynchronously
    /// </summary>
    /// <remarks>
    /// <paramref name="icoHandle"/> will not be destroyed, the responsibility lies with the caller
    /// </remarks>
    /// <param name="icoHandle">The handle of a .ico file</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to stop this <see cref="NotifyIcon"/> instance</param>
    /// <returns><see cref="NotifyIcon"/></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Task<NotifyIcon> RunAsync(nint icoHandle, CancellationToken cancellationToken)
    {
        return icoHandle == nint.Zero
            ? throw new ArgumentNullException(nameof(icoHandle), "The handle cannot be null")
            : RunAsync(icoHandle, false, [], cancellationToken);
    }

    /// <summary>
    /// Creates and runs a <see cref="NotifyIcon"/> instance asynchronously
    /// </summary>
    /// <remarks>
    /// <paramref name="icoHandle"/> will not be destroyed, the responsibility lies with the caller
    /// </remarks>
    /// <param name="icoHandle">The handle of a .ico file</param>
    /// <param name="menuItems">The initial <see cref="MenuItemCollection"/></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to stop this <see cref="NotifyIcon"/> instance</param>
    /// <returns><see cref="NotifyIcon"/></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Task<NotifyIcon> RunAsync(nint icoHandle, MenuItemCollection menuItems, CancellationToken cancellationToken)
    {
        return icoHandle == nint.Zero
            ? throw new ArgumentNullException(nameof(icoHandle), "The handle cannot be null")
            : RunAsync(icoHandle, false, menuItems, cancellationToken);
    }
}