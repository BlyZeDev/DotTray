namespace DotTray;

using DotTray.Internal;
using DotTray.Internal.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

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

    private IEnumerable<IMenuItem>? menuItems;
    public IEnumerable<IMenuItem> MenuItems
    {
        get => menuItems ?? [];
        set
        {
            menuItems = value;
            Native.PostMessage(hWnd, Native.WM_APP_TRAYICON_REBUILD, 0, 0);
        }
    }

    private string? toolTip;
    public string ToolTip
    {
        get => toolTip ?? "";
        set
        {
            if (toolTip == value) return;

            toolTip = value;
            Native.PostMessage(hWnd, Native.WM_APP_TRAYICON_TOOLTIP, 0, 0);
        }
    }

    private NotifyIcon(nint iconHandle, bool needIconDestroy, CancellationToken cancellationToken)
    {
        _iconHandle = iconHandle;
        _needsIconDestroy = needIconDestroy;

        _menuActions = [];
        _subMenus = [];

        menuRefreshQueued = false;
        nextCommandId = 1000;

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

            ShowIcon();

            if (menuItems is not null) Native.PostMessage(hWnd, Native.WM_APP_TRAYICON_REBUILD, 0, 0);
            if (toolTip is not null) Native.PostMessage(hWnd, Native.WM_APP_TRAYICON_TOOLTIP, 0, 0);

            using (var registration = cancellationToken.Register(() => Native.PostMessage(hWnd, Native.WM_QUIT, 0, 0)))
            {
                while (Native.GetMessage(out var message, hWnd, 0, 0))
                {
                    Native.TranslateMessage(ref message);
                    Native.DispatchMessage(ref message);
                }
            }
        });
        _trayLoopThread.Start();
    }

    public void Dispose()
    {
        RemoveIcon();
        MonitorMenuItems(MenuItems, true);

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

        if (_trayLoopThread.IsAlive) _trayLoopThread.Join();
    }

    public static NotifyIcon Run(string icoPath, CancellationToken cancellationToken)
    {
        if (!File.Exists(icoPath)) throw new FileNotFoundException("The .ico file could not be found", icoPath);

        var handle = Native.LoadImage(nint.Zero, icoPath, Native.IMAGE_ICON, 0, 0, Native.LR_LOADFROMFILE);
        if (handle == nint.Zero) throw new FileLoadException("The .ico file could not be loaded", icoPath);

        return Run(handle, true, cancellationToken);
    }

    public static NotifyIcon Run(nint iconHandle, CancellationToken cancellationToken)
        => Run(iconHandle, false, cancellationToken);

    private static NotifyIcon Run(nint iconHandle, bool needIconDestroy, CancellationToken cancellationToken)
        => new NotifyIcon(iconHandle, needIconDestroy, cancellationToken);
}