namespace DotTray;

using DotTray.Internal;
using DotTray.Internal.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

[SupportedOSPlatform("Windows")]
public sealed partial class NotifyIcon : IDisposable
{
    private const string WindowClassName = $"{nameof(DotTray)}NotifyIconWindow";

    private readonly nint _iconHandle;
    private readonly bool _needsIconDestroy;
    private readonly Thread _trayLoopThread;

    private readonly Dictionary<int, Action> _menuActions;
    private readonly Dictionary<string, nint> _subMenus;

    private nint instanceHandle;
    private GCHandle thisHandle;
    private nint hWnd;
    private nint trayMenu;

    private bool menuRefreshQueued;
    private int nextCommandId;

    public IEnumerable<IMenuItem> MenuItems { get; private set; }

    public string ToolTip { get; private set; }

    private NotifyIcon(nint iconHandle, bool needIconDestroy, Action onInitializationFinished, CancellationToken cancellationToken)
    {
        _iconHandle = iconHandle;
        _needsIconDestroy = needIconDestroy;

        _menuActions = [];
        _subMenus = [];

        menuRefreshQueued = false;
        nextCommandId = 1000;

        MenuItems = [];
        ToolTip = "";

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

            ShowIcon();

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
            }
        });
        _trayLoopThread.SetApartmentState(ApartmentState.STA);
        _trayLoopThread.Start();
    }

    public void SetMenuItems(IEnumerable<IMenuItem> menuItems)
    {
        MenuItems = menuItems;
        Native.PostMessage(hWnd, Native.WM_APP_TRAYICON_REBUILD, 0, 0);

        MonitorMenuItems(MenuItems);
    }

    public void SetToolTip(string toolTip)
    {
        if (ToolTip.Equals(toolTip, StringComparison.Ordinal)) return;

        ToolTip = toolTip;
        Native.PostMessage(hWnd, Native.WM_APP_TRAYICON_TOOLTIP, 0, 0);
    }

    public void Dispose()
    {
        Native.PostMessage(hWnd, Native.WM_APP_TRAYICON_QUIT, 0, 0);

        RemoveIcon();
        MonitorMenuItems(MenuItems, true);

        if (_trayLoopThread.IsAlive) _trayLoopThread.Join();

        if (_needsIconDestroy) Native.DestroyIcon(_iconHandle);

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

    public static NotifyIcon Run(string icoPath, CancellationToken cancellationToken)
    {
        var iconHandle = PrepareIconHandle(icoPath);

        return Run(iconHandle, true, cancellationToken);
    }

    public static NotifyIcon Run(nint iconHandle, CancellationToken cancellationToken)
        => Run(iconHandle, false, cancellationToken);

    public static Task<NotifyIcon> RunAsync(string icoPath, CancellationToken cancellationToken)
    {
        var iconHandle = PrepareIconHandle(icoPath);

        return RunAsync(iconHandle, true, cancellationToken);
    }

    public static Task<NotifyIcon> RunAsync(nint iconHandle, CancellationToken cancellationToken)
        => RunAsync(iconHandle, false, cancellationToken);
}