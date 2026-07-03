namespace DotTray;

using DotTray.Abstract;
using DotTray.Internal;
using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using DotTray.Popup;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

public sealed partial class NotifyIcon<THandler>
{
    private const uint WM_APP_TRAYICON_CALLBACK = PInvoke.WM_APP + 1;
    private const uint WM_APP_TRAYICON_TOOLTIP = PInvoke.WM_APP + 2;
    private const uint WM_APP_TRAYICON_VISIBILITY = PInvoke.WM_APP + 3;
    private const uint WM_APP_TRAYICON_BALLOON = PInvoke.WM_APP + 4;

    private readonly nint _icoHandle;
    private readonly INotifyIconHandler _handler;

    private readonly nint _popupWindowClassName;
    private readonly nint _instanceHandle;
    private readonly Thread _thread;

    private nint hWnd;

    private BalloonNotification? nextBalloon;

    internal NotifyIcon(nint icoHandle, INotifyIconHandler? handler, Action onInitializationFinished, CancellationToken token)
    {
        NotifyIcon.TotalIcons++;
        Id = Guid.CreateVersion7();

        _icoHandle = icoHandle;
        _handler = handler ?? new NativePopupMenuHandler();

        ToolTip = null;
        IsVisible = true;

        var windowClassNameString = $"{nameof(DotTray)}NotifyIconWindow{Id}";
        var windowClassName = Marshal.StringToHGlobalUni(windowClassNameString);
        _popupWindowClassName = Marshal.StringToHGlobalUni($"{windowClassNameString}_Popup");

        _instanceHandle = PInvoke.GetModuleHandle(null);
        NotifyIconException.ThrowIfNull(_instanceHandle, "Acquiring module handle failed");

        _thread = new Thread(() =>
        {
            var result = PInvoke.SetThreadDpiAwarenessContext(PInvoke.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
            NotifyIconException.ThrowIfNull(result, "Setting the DPI awareness for this thread failed");

            if (NotifyIcon.GdipToken == nint.Zero)
            {
                var input = new GDIPLUSSTARTUPINPUT
                {
                    GdiplusVersion = 1
                };
                var gdipStatus = (PInvoke.GdiPlusStatus)PInvoke.GdiplusStartup(out NotifyIcon.GdipToken, ref input, out _);
                NotifyIconException.ThrowIfNotOk(gdipStatus, "GDI+ startup failed");
            }

            var wndProc = new PInvoke.WndProc(WndProcFunc);
            var wndClass = new WNDCLASS
            {
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProc),
                hInstance = _instanceHandle,
                lpszClassName = windowClassName
            };
            var atom = PInvoke.RegisterClass(ref wndClass);
            NotifyIconException.ThrowIfZero(atom, "Registering the window class failed");

            var popupWndProc = new PInvoke.WndProc((hWnd, msg, wParam, lParam) => PInvoke.DefWindowProc(hWnd, msg, wParam, lParam));
            var popupWndClass = new WNDCLASS
            {
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(popupWndProc),
                hInstance = _instanceHandle,
                lpszClassName = _popupWindowClassName
            };
            atom = PInvoke.RegisterClass(ref popupWndClass);
            NotifyIconException.ThrowIfZero(atom, "Registering the window class failed");

            hWnd = PInvoke.CreateWindowEx(0, windowClassName, nint.Zero, 0, 0, 0, 0, 0, nint.Zero, nint.Zero, _instanceHandle, nint.Zero);
            NotifyIconException.ThrowIfNull(hWnd, "Creating a window failed");

            var iconData = new NOTIFYICONDATA
            {
                cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                hWnd = hWnd,
                guidItem = Id,
                uFlags = PInvoke.NIF_MESSAGE | PInvoke.NIF_ICON | PInvoke.NIF_GUID,
                uCallbackMessage = WM_APP_TRAYICON_CALLBACK,
                hIcon = _icoHandle
            };
            var success = PInvoke.Shell_NotifyIcon(PInvoke.NIM_ADD, ref iconData);
            NotifyIconException.ThrowIfFalse(success, "Creating a notification icon failed");

            iconData.uTimeoutOrVersion = 4;
            success = PInvoke.Shell_NotifyIcon(PInvoke.NIM_SETVERSION, ref iconData);
            NotifyIconException.ThrowIfFalse(success, "Setting the version of a notification icon failed");

            onInitializationFinished();

            using (var registration = token.Register(() => PInvoke.PostMessage(hWnd, PInvoke.WM_CLOSE, 0, 0)))
            {
                success = PInvoke.PostMessage(hWnd, WM_APP_TRAYICON_TOOLTIP, 0, 0);
                NotifyIconException.ThrowIfFalse(success, "Posting a message failed");

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
                success = PInvoke.Shell_NotifyIcon(PInvoke.NIM_DELETE, ref iconData);
                NotifyIconException.ThrowIfFalse(success, "Deleting a notification icon failed");

                success = PInvoke.DestroyIcon(_icoHandle);
                NotifyIconException.ThrowIfFalse(success, "Destroying the icon failed");

                if (hWnd != nint.Zero)
                {
                    success = PInvoke.DestroyWindow(hWnd);
                    NotifyIconException.ThrowIfFalse(success, "Destroying the window failed");
                    hWnd = nint.Zero;

                    success = PInvoke.UnregisterClass(_popupWindowClassName, _instanceHandle);
                    NotifyIconException.ThrowIfFalse(success, "Unregistering a class failed");
                    success = PInvoke.UnregisterClass(windowClassName, _instanceHandle);
                    NotifyIconException.ThrowIfFalse(success, "Unregistering a class failed");

                    Marshal.FreeHGlobal(windowClassName);
                    Marshal.FreeHGlobal(_popupWindowClassName);
                }
            }

            GC.KeepAlive(wndProc);
            GC.KeepAlive(popupWndProc);
        });
        _thread.Name = $"{nameof(NotifyIcon)}::{Id}";
        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();
    }

    private unsafe nint WndProcFunc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case WM_APP_TRAYICON_CALLBACK: HandleCallback(wParam, lParam); break;

            case WM_APP_TRAYICON_TOOLTIP: HandleToolTip(hWnd); break;

            case WM_APP_TRAYICON_VISIBILITY: HandleVisibility(hWnd); break;

            case WM_APP_TRAYICON_BALLOON: HandleBalloon(hWnd); break;

            case PInvoke.WM_DESTROY: PInvoke.PostQuitMessage(0); return 0;
        }

        return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private void HandleCallback(nint wParam, nint lParam)
    {
        var interaction = new NotifyIconInteractedEventArgs
        {
            Type = (InteractionType)(uint)(lParam & 0xFFFF),
            MousePosition = new Point((short)(wParam & 0xFFFF), (short)((wParam >> 16) & 0xFFFF))
        };

        Interacted?.Invoke(interaction);
        _handler.HandleInteraction(this, interaction);
    }

    private unsafe void HandleToolTip(nint hWnd)
    {
        var iconData = new NOTIFYICONDATA
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = hWnd,
            guidItem = Id,
            uFlags = PInvoke.NIF_TIP | PInvoke.NIF_SHOWTIP | PInvoke.NIF_GUID
        };
        NativeString.WriteFixed(iconData.szTip, NOTIFYICONDATA.SZTIP_LENGTH, ToolTip ?? "");

        var success = PInvoke.Shell_NotifyIcon(PInvoke.NIM_MODIFY, ref iconData);
        NotifyIconException.ThrowIfFalse(success, "Modifying the notification icon failed");
    }

    private void HandleVisibility(nint hWnd)
    {
        var iconData = new NOTIFYICONDATA
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = hWnd,
            guidItem = Id,
            uFlags = PInvoke.NIF_STATE | PInvoke.NIF_GUID,
            dwState = IsVisible ? 0 : PInvoke.NIS_HIDDEN,
            dwStateMask = PInvoke.NIS_HIDDEN
        };

        var success = PInvoke.Shell_NotifyIcon(PInvoke.NIM_MODIFY, ref iconData);
        NotifyIconException.ThrowIfFalse(success, "Modifying the notification icon failed");
    }

    private unsafe void HandleBalloon(nint hWnd)
    {
        if (nextBalloon is null) return;

        var iconData = new NOTIFYICONDATA
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = hWnd,
            guidItem = Id,
            hBalloonIcon = (nextBalloon.Icon is BalloonNotificationIcon.User) ? _icoHandle : nint.Zero,
            uFlags = PInvoke.NIF_INFO | PInvoke.NIF_GUID,
            dwInfoFlags = (uint)nextBalloon.Icon | (nextBalloon.NoSound ? PInvoke.NIIF_NOSOUND : 0) | (nextBalloon.Icon is BalloonNotificationIcon.User ? PInvoke.NIIF_LARGE_ICON : 0)
        };

        NativeString.WriteFixed(iconData.szInfoTitle, NOTIFYICONDATA.SZINFOTITLE_LENGTH, nextBalloon.Title);
        NativeString.WriteFixed(iconData.szInfo, NOTIFYICONDATA.SZINFO_LENGTH, nextBalloon.Message);

        nextBalloon = null;

        var success = PInvoke.Shell_NotifyIcon(PInvoke.NIM_MODIFY, ref iconData);
        NotifyIconException.ThrowIfFalse(success, "Modifying the notification icon failed");
    }
}