namespace DotTray;

using DotTray.Internal;
using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Represents a Notification Icon that is displayed in the Taskbar
/// </summary>
/// <remarks>
/// To get the best possible result it's recommend to use a 16x16 or 32x32 icon with a 32-bit color depth including alpha channel.<br/>
/// Using other sizes or color depths may lead to unexpected results or poor quality rendering.
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed partial class NotifyIcon : IDisposable
{
    private static readonly string DefaultToolTip = "";
    private static readonly MouseButton DefaultMouseButtons = MouseButton.Left | MouseButton.Right;
    private const float DefaultFontSize = 20f;
    private static readonly TrayColor DefaultPopupMenuColor = new TrayColor(40, 40, 40);

    private static readonly Action<MenuItem> DefaultMenuItemConfig = x =>
    {
        x.IsChecked = null;
        x.IsDisabled = false;
        x.BackgroundColor = TrayColor.Transparent;
        x.BackgroundHoverColor = new TrayColor(0, 120, 215);
        x.BackgroundDisabledColor = TrayColor.Gray;
        x.TextColor = TrayColor.White;
        x.TextHoverColor = TrayColor.White;
        x.TextDisabledColor = new TrayColor(109, 109, 109);
    };

    private static readonly Action<SeparatorItem> DefaultSeparatorItemConfig = x =>
    {
        x.BackgroundColor = TrayColor.Transparent;
        x.LineColor = TrayColor.White;
        x.LineThickness = 1f;
    };

    private static uint totalIcons;
    private static nint gdipToken;

    private readonly nint _popupWindowClassName;
    private readonly Thread _trayLoopThread;

    private nint baseIcoHandle;
    private nint shownIcoHandle;

    private nint instanceHandle;
    private nint hWnd;

    private POINT lastMousePos;
    private PopupMenuSession? popupMenuSession;
    private BalloonNotification? nextBalloon;

    /// <summary>
    /// The unique identifier of this <see cref="NotifyIcon"/> instance
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// The <see cref="MenuItemCollection"/> of this <see cref="NotifyIcon"/> instance
    /// </summary>
    public MenuItemCollection MenuItems { get; }

    /// <summary>
    /// The tooltip text of this <see cref="NotifyIcon"/> instance
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="string.Empty"/>
    /// </remarks>
    public string ToolTip { get; private set; }

    /// <summary>
    /// The current visibility of this <see cref="NotifyIcon"/>.<br/>
    /// <see langword="true"/> if it is visible, otherwise <see langword="false"/>
    /// </summary>
    /// <remarks>
    /// The default value is <see langword="true"/>
    /// </remarks>
    public bool IsVisible { get; private set; }

    /// <summary>
    /// The mouse buttons that are allowed to interact with this <see cref="NotifyIcon"/> instance
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="MouseButton.Left"/> | <see cref="MouseButton.Right"/>
    /// </remarks>
    public MouseButton MouseButtons { get; set; }

    /// <summary>
    /// The font size in device-independent pixels (DIP) of the popup menu for this <see cref="NotifyIcon"/> instance
    /// </summary>
    public float FontSize { get; private set; }

    /// <summary>
    /// The background color of the popup menu for this <see cref="NotifyIcon"/> instance
    /// </summary>
    public TrayColor PopupMenuColor { get; private set; }

    /// <summary>
    /// The badge displayed on top of the icon for this <see cref="NotifyIcon"/> instance
    /// </summary>
    /// <remarks>
    /// <see langword="null"/>, if no badge is displayed
    /// </remarks>
    public NotifyIconBadge? Badge { get; private set; }

    /// <summary>
    /// Fired if the icon popup menu is showing by clicking <see cref="MouseButtons"/>
    /// </summary>
    public event Action<MouseButton>? PopupShowing;

    /// <summary>
    /// Fired if the icon popup menu is hiding
    /// </summary>
    public event Action? PopupHiding;

    private unsafe NotifyIcon(nint icoHandle, Action onInitializationFinished, Action<MenuItem>? defaultMenuItemConfig, Action<SeparatorItem>? defaultSeparatorItemConfig, CancellationToken cancellationToken)
    {
        baseIcoHandle = icoHandle;
        shownIcoHandle = PInvoke.CopyIcon(baseIcoHandle);

        totalIcons++;
        Id = Guid.CreateVersion7();

        var windowClassNameString = $"{nameof(DotTray)}NotifyIconWindow{Id}";
        var windowClassName = Marshal.StringToHGlobalUni(windowClassNameString);
        _popupWindowClassName = Marshal.StringToHGlobalUni($"{windowClassNameString}_Popup");

        MenuItems = new MenuItemCollection(DefaultMenuItemConfig + defaultMenuItemConfig, DefaultSeparatorItemConfig + defaultSeparatorItemConfig);
        ToolTip = DefaultToolTip;
        IsVisible = true;
        MouseButtons = DefaultMouseButtons;
        FontSize = DefaultFontSize;
        PopupMenuColor = DefaultPopupMenuColor;
        Badge = null;

        _trayLoopThread = new Thread(() =>
        {
            PInvoke.SetThreadDpiAwarenessContext(PInvoke.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);

            if (gdipToken == nint.Zero)
            {
                var input = new GDIPLUSSTARTUPINPUT
                {
                    GdiplusVersion = 1
                };
                _ = PInvoke.GdiplusStartup(out gdipToken, ref input, out _);
            }

            instanceHandle = PInvoke.GetModuleHandle(null);

            var wndProc = new PInvoke.WndProc(WndProcFunc);
            var wndClass = new WNDCLASS
            {
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProc),
                hInstance = instanceHandle,
                lpszClassName = windowClassName
            };
            PInvoke.RegisterClass(ref wndClass);

            var popupWndProc = new PInvoke.WndProc((hWnd, msg, wParam, lParam) => PInvoke.DefWindowProc(hWnd, msg, wParam, lParam));
            var popupWndClass = new WNDCLASS
            {
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(popupWndProc),
                hInstance = instanceHandle,
                lpszClassName = _popupWindowClassName
            };
            PInvoke.RegisterClass(ref popupWndClass);

            hWnd = PInvoke.CreateWindowEx(0, windowClassName, nint.Zero, 0, 0, 0, 0, 0, nint.Zero, nint.Zero, instanceHandle, nint.Zero);

            var iconData = new NOTIFYICONDATA
            {
                cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                hWnd = hWnd,
                guidItem = Id,
                uFlags = PInvoke.NIF_MESSAGE | PInvoke.NIF_ICON | PInvoke.NIF_GUID,
                uCallbackMessage = PInvoke.WM_APP_TRAYICON_CLICK,
                hIcon = shownIcoHandle
            };
            PInvoke.Shell_NotifyIcon(PInvoke.NIM_ADD, ref iconData);

            onInitializationFinished();

            using (var registration = cancellationToken.Register(() => PInvoke.PostMessage(hWnd, PInvoke.WM_CLOSE, 0, 0)))
            {
                PInvoke.PostMessage(hWnd, PInvoke.WM_APP_TRAYICON_TOOLTIP, 0, 0);

                while (PInvoke.GetMessage(out var message, nint.Zero, 0, 0))
                {
                    PInvoke.TranslateMessage(ref message);
                    PInvoke.DispatchMessage(ref message);
                }

                iconData = new NOTIFYICONDATA
                {
                    cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                    hWnd = hWnd,
                    guidItem = Id,
                    uFlags = PInvoke.NIF_GUID
                };
                PInvoke.Shell_NotifyIcon(PInvoke.NIM_DELETE, ref iconData);

                PInvoke.DestroyIcon(baseIcoHandle);
                PInvoke.DestroyIcon(shownIcoHandle);

                if (hWnd != nint.Zero)
                {
                    PInvoke.DestroyWindow(hWnd);
                    hWnd = nint.Zero;

                    PInvoke.UnregisterClass(_popupWindowClassName, instanceHandle);
                    PInvoke.UnregisterClass(windowClassName, instanceHandle);

                    Marshal.FreeHGlobal(windowClassName);
                    Marshal.FreeHGlobal(_popupWindowClassName);

                    instanceHandle = nint.Zero;
                }
            }

            GC.KeepAlive(wndProc);
            GC.KeepAlive(popupWndProc);
        });
        _trayLoopThread.Name = $"{nameof(NotifyIcon)}::{Id}";
        _trayLoopThread.SetApartmentState(ApartmentState.STA);
        _trayLoopThread.Start();
    }

    /// <summary>
    /// Sets the Icon for this <see cref="NotifyIcon"/> instance
    /// </summary>
    /// <param name="icoPath">The path to a .ico file</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="FileLoadException"></exception>
    public void SetIcon(string icoPath) => SetIcon(PrepareIconHandle(icoPath));

    /// <summary>
    /// Sets the Icon for this <see cref="NotifyIcon"/> instance
    /// </summary>
    /// <remarks>
    /// If <paramref name="icoHandle"/> == <see cref="nint.Zero"/> the icon will be invisible.<br/>
    /// <paramref name="icoHandle"/> will not be destroyed, the responsibility lies with the caller
    /// </remarks>
    /// <param name="icoHandle">The handle of a .ico file</param>
    public void SetIcon(nint icoHandle) => PInvoke.PostMessage(hWnd, PInvoke.WM_APP_TRAYICON_ICON, icoHandle, nint.Zero);

    /// <summary>
    /// Sets the <see cref="ToolTip"/> for this <see cref="NotifyIcon"/> instance
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
        PInvoke.PostMessage(hWnd, PInvoke.WM_APP_TRAYICON_TOOLTIP, nint.Zero, nint.Zero);
    }

    /// <summary>
    /// Sets the <see cref="FontSize"/> of the popup menu for this <see cref="NotifyIcon"/> instance
    /// </summary>
    /// <param name="fontSize">The size to set for <see cref="FontSize"/></param>
    public void SetFontSize(float fontSize)
    {
        if (FontSize == fontSize) return;

        FontSize = fontSize;
        AttemptSessionRestart();
    }

    /// <summary>
    /// Sets the <see cref="PopupMenuColor"/> of the popup menu for this <see cref="NotifyIcon"/> instance
    /// </summary>
    /// <param name="popupMenuColor">The color to set for <see cref="PopupMenuColor"/></param>
    public void SetPopupMenuColor(TrayColor popupMenuColor)
    {
        if (PopupMenuColor == popupMenuColor) return;

        PopupMenuColor = popupMenuColor;
        AttemptSessionRestart();
    }

    /// <summary>
    /// Sets the <see cref="Badge"/> for this <see cref="NotifyIcon"/> instance
    /// </summary>
    /// <param name="badge">The badge to set for <see cref="Badge"/></param>
    public void SetBadge(NotifyIconBadge? badge)
    {
        if (Badge == badge) return;

        Badge = badge;
        SetIcon(baseIcoHandle);
    }

    /// <summary>
    /// Hides this <see cref="NotifyIcon"/> instance
    /// </summary>
    public void Hide()
    {
        if (!IsVisible) return;

        IsVisible = false;
        PInvoke.PostMessage(hWnd, PInvoke.WM_APP_TRAYICON_VISIBILITY, 0, 0);
    }

    /// <summary>
    /// Shows this <see cref="NotifyIcon"/> instance
    /// </summary>
    public void Show()
    {
        if (IsVisible) return;

        IsVisible = true;
        PInvoke.PostMessage(hWnd, PInvoke.WM_APP_TRAYICON_VISIBILITY, 0, 0);
    }

    /// <summary>
    /// Shows a balloon notification
    /// </summary>
    /// <param name="balloon">The balloon notification to show</param>
    public void ShowBalloon(BalloonNotification balloon)
    {
        nextBalloon = balloon with
        {
            Title = balloon.Title.Length > NOTIFYICONDATA.SZINFOTITLE_LENGTH ? balloon.Title[..NOTIFYICONDATA.SZINFOTITLE_LENGTH] : balloon.Title,
            Message = balloon.Message.Length > NOTIFYICONDATA.SZINFO_LENGTH ? balloon.Message[..NOTIFYICONDATA.SZINFO_LENGTH] : balloon.Message
        };

        PInvoke.PostMessage(hWnd, PInvoke.WM_APP_TRAYICON_BALLOON, 0, 0);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        PInvoke.PostMessage(hWnd, PInvoke.WM_CLOSE, 0, 0);

        if (_trayLoopThread.IsAlive) _trayLoopThread.Join();

        totalIcons--;
        if (totalIcons == 0)
        {
            PInvoke.GdiplusShutdown(gdipToken);
            gdipToken = nint.Zero;
        }
    }

    /// <summary>
    /// Creates and runs a <see cref="NotifyIcon"/> instance synchronously
    /// </summary>
    /// <remarks>
    /// This will block until the <see cref="NotifyIcon"/> instance is ready or an <see cref="Exception"/> occurs
    /// </remarks>
    /// <param name="icoPath">The path to a .ico file</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to stop this <see cref="NotifyIcon"/> instance</param>
    /// <param name="defaultMenuItemConfig">The default configuration for <see cref="MenuItem"/> instances</param>
    /// <param name="defaultSeparatorItemConfig">The default configuration for <see cref="SeparatorItem"/> instances</param>
    /// <returns><see cref="NotifyIcon"/></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="FileLoadException"></exception>
    public static NotifyIcon Run(string icoPath, CancellationToken cancellationToken, Action<MenuItem>? defaultMenuItemConfig = null, Action<SeparatorItem>? defaultSeparatorItemConfig = null)
        => RunInternal(PrepareIconHandle(icoPath), defaultMenuItemConfig, defaultSeparatorItemConfig, cancellationToken);

    /// <summary>
    /// Creates and runs a <see cref="NotifyIcon"/> instance synchronously
    /// </summary>
    /// <remarks>
    /// This will block until the <see cref="NotifyIcon"/> instance is ready or an <see cref="Exception"/> occurs.<br/>
    /// <paramref name="icoHandle"/> will not be destroyed, the responsibility lies with the caller
    /// </remarks>
    /// <param name="icoHandle">The handle of a .ico file</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to stop this <see cref="NotifyIcon"/> instance</param>
    /// <param name="defaultMenuItemConfig">The default configuration for <see cref="MenuItem"/> instances</param>
    /// <param name="defaultSeparatorItemConfig">The default configuration for <see cref="SeparatorItem"/> instances</param>
    /// <returns><see cref="NotifyIcon"/></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static NotifyIcon Run(nint icoHandle, CancellationToken cancellationToken, Action<MenuItem>? defaultMenuItemConfig = null, Action<SeparatorItem>? defaultSeparatorItemConfig = null)
    {
        if (icoHandle == nint.Zero) throw new ArgumentNullException(nameof(icoHandle), "The handle cannot be null");

        icoHandle = PInvoke.CopyIcon(icoHandle);

        return icoHandle == nint.Zero
            ? throw new ArgumentNullException(nameof(icoHandle), "The handle cannot be null")
            : RunInternal(icoHandle, defaultMenuItemConfig, defaultSeparatorItemConfig, cancellationToken);
    }

    /// <summary>
    /// Creates and runs a <see cref="NotifyIcon"/> instance asynchronously
    /// </summary>
    /// <param name="icoPath">The path to a .ico file</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to stop this <see cref="NotifyIcon"/> instance</param>
    /// <param name="defaultMenuItemConfig">The default configuration for <see cref="MenuItem"/> instances</param>
    /// <param name="defaultSeparatorItemConfig">The default configuration for <see cref="SeparatorItem"/> instances</param>
    /// <returns><see cref="NotifyIcon"/></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="FileLoadException"></exception>
    public static Task<NotifyIcon> RunAsync(string icoPath, CancellationToken cancellationToken, Action<MenuItem>? defaultMenuItemConfig = null, Action<SeparatorItem>? defaultSeparatorItemConfig = null)
        => RunInternalAsync(PrepareIconHandle(icoPath), defaultMenuItemConfig, defaultSeparatorItemConfig, cancellationToken);

    /// <summary>
    /// Creates and runs a <see cref="NotifyIcon"/> instance asynchronously
    /// </summary>
    /// <remarks>
    /// <paramref name="icoHandle"/> will not be destroyed, the responsibility lies with the caller
    /// </remarks>
    /// <param name="icoHandle">The handle of a .ico file</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to stop this <see cref="NotifyIcon"/> instance</param>
    /// <param name="defaultMenuItemConfig">The default configuration for <see cref="MenuItem"/> instances</param>
    /// <param name="defaultSeparatorItemConfig">The default configuration for <see cref="SeparatorItem"/> instances</param>
    /// <returns><see cref="NotifyIcon"/></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Task<NotifyIcon> RunAsync(nint icoHandle, CancellationToken cancellationToken, Action<MenuItem>? defaultMenuItemConfig = null, Action<SeparatorItem>? defaultSeparatorItemConfig = null)
    {
        if (icoHandle == nint.Zero) throw new ArgumentNullException(nameof(icoHandle), "The handle cannot be null");

        icoHandle = PInvoke.CopyIcon(icoHandle);

        return icoHandle == nint.Zero
            ? throw new ArgumentNullException(nameof(icoHandle), "The handle cannot be null")
            : RunInternalAsync(icoHandle, defaultMenuItemConfig, defaultSeparatorItemConfig, cancellationToken);
    }
}