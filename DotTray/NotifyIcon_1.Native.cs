namespace DotTray;

using DotTray.Internal;
using DotTray.Internal.Models;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

public sealed partial class NotifyIcon<THandler>
{
    private const uint WM_APP_TRAYICON_CALLBACK = PInvoke.WM_APP + 1;
    private const uint WM_APP_TRAYICON_TOOLTIP = PInvoke.WM_APP + 2;
    private const uint WM_APP_TRAYICON_VISIBILITY = PInvoke.WM_APP + 3;
    private const uint WM_APP_TRAYICON_BALLOON = PInvoke.WM_APP + 4;

    private static readonly uint WM_TASKBARCREATED = PInvoke.RegisterWindowMessage("TaskbarCreated");

    private readonly nint _hIco;
    private readonly nint _hInstance;
    private readonly Thread _thread;

    private nint hWnd;

    private BalloonNotification? nextBalloon;

    internal NotifyIcon(nint icoHandle, THandler handler, Action onInitializationFinished, CancellationToken token)
    {
        NotifyIcon.TotalIcons++;
        Id = Guid.CreateVersion7();

        _hIco = icoHandle;
        Handler = handler;

        ToolTip = null;
        IsVisible = true;

        var windowClassNameString = $"{nameof(DotTray)}{nameof(NotifyIcon)}Window{Id}";
        var windowClassName = Marshal.StringToHGlobalUni(windowClassNameString);

        _hInstance = NativeLibrary.GetMainProgramHandle();
        _thread = new Thread(() =>
        {
            var result = PInvoke.SetThreadDpiAwarenessContext(PInvoke.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
            if (result == nint.Zero) throw new Win32Exception(Marshal.GetLastPInvokeError());

            var wndProc = new PInvoke.WndProc(WndProcFunc);
            var wndClass = new WNDCLASS
            {
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProc),
                hInstance = _hInstance,
                lpszClassName = windowClassName
            };
            var atom = PInvoke.RegisterClass(ref wndClass);
            if (atom == 0) throw new Win32Exception(Marshal.GetLastPInvokeError());

            hWnd = PInvoke.CreateWindowEx(0, windowClassName, nint.Zero, 0, 0, 0, 0, 0, nint.Zero, nint.Zero, _hInstance, nint.Zero);
            if (hWnd == nint.Zero) throw new Win32Exception(Marshal.GetLastPInvokeError());

            var iconData = new NOTIFYICONDATA
            {
                cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                hWnd = hWnd,
                guidItem = Id,
                uFlags = PInvoke.NIF_MESSAGE | PInvoke.NIF_ICON | PInvoke.NIF_GUID,
                uCallbackMessage = WM_APP_TRAYICON_CALLBACK,
                hIcon = _hIco
            };
            var success = PInvoke.Shell_NotifyIcon(PInvoke.NIM_ADD, ref iconData);
            if (!success) throw new Win32Exception(Marshal.GetLastPInvokeError());

            iconData.uTimeoutOrVersion = 4;
            success = PInvoke.Shell_NotifyIcon(PInvoke.NIM_SETVERSION, ref iconData);
            if (!success) throw new Win32Exception(Marshal.GetLastPInvokeError());

            onInitializationFinished();

            using (var registration = token.Register(() => PInvoke.PostMessage(hWnd, PInvoke.WM_CLOSE, 0, 0)))
            {
                success = PInvoke.PostMessage(hWnd, WM_APP_TRAYICON_TOOLTIP, 0, 0);
                if (!success) throw new Win32Exception(Marshal.GetLastPInvokeError());

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
                if (!success) throw new Win32Exception(Marshal.GetLastPInvokeError());

                success = PInvoke.DestroyIcon(_hIco);
                if (!success) throw new Win32Exception(Marshal.GetLastPInvokeError());

                success = PInvoke.UnregisterClass(windowClassName, _hInstance);
                if (!success) throw new Win32Exception(Marshal.GetLastPInvokeError());

                Marshal.FreeHGlobal(windowClassName);
            }

            GC.KeepAlive(wndProc);
        })
        {
            Name = $"{nameof(NotifyIcon)}::{Id}"
        };
        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();
    }

    private nint WndProcFunc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        if (msg == WM_TASKBARCREATED)
        {
            HandleRestore(hWnd);
            return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        switch (msg)
        {
            case PInvoke.WM_POWERBROADCAST when wParam is PInvoke.PBT_APMRESUMEAUTOMATIC or PInvoke.PBT_APMRESUMESUSPEND:
                HandleRestore(hWnd);
                return 1;

            case WM_APP_TRAYICON_CALLBACK: HandleCallback(wParam, lParam); return 0;

            case WM_APP_TRAYICON_TOOLTIP: HandleToolTip(hWnd); return 0;

            case WM_APP_TRAYICON_VISIBILITY: HandleVisibility(hWnd); return 0;

            case WM_APP_TRAYICON_BALLOON: HandleBalloon(hWnd); return 0;

            case PInvoke.WM_CLOSE: PInvoke.DestroyWindow(hWnd); return 0;

            case PInvoke.WM_DESTROY:
                this.hWnd = nint.Zero;
                PInvoke.PostQuitMessage(0);
                return 0;
        }

        return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private void HandleRestore(nint hWnd)
    {
        var iconData = new NOTIFYICONDATA
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = hWnd,
            guidItem = Id,
            uFlags = PInvoke.NIF_MESSAGE | PInvoke.NIF_ICON | PInvoke.NIF_GUID,
            uCallbackMessage = WM_APP_TRAYICON_CALLBACK,
            hIcon = _hIco
        };

        var success = PInvoke.Shell_NotifyIcon(PInvoke.NIM_ADD, ref iconData);
        if (!success) PInvoke.Shell_NotifyIcon(PInvoke.NIM_MODIFY, ref iconData);

        iconData.uTimeoutOrVersion = 4;
        PInvoke.Shell_NotifyIcon(PInvoke.NIM_SETVERSION, ref iconData);

        PInvoke.PostMessage(hWnd, WM_APP_TRAYICON_TOOLTIP, 0, 0);
        PInvoke.PostMessage(hWnd, WM_APP_TRAYICON_VISIBILITY, 0, 0);
    }

    private void HandleCallback(nint wParam, nint lParam)
    {
        var interaction = new NotifyIconInteractedEventArgs
        {
            Type = (IconInteractionType)(uint)(lParam & 0xFFFF),
            MousePosition = new MousePosition((short)(wParam & 0xFFFF), (short)((wParam >> 16) & 0xFFFF))
        };

        Interacted?.Invoke(interaction);
        Handler.HandleInteraction(this, interaction);
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
        if (!success) throw new Win32Exception(Marshal.GetLastPInvokeError());
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
        if (!success) throw new Win32Exception(Marshal.GetLastPInvokeError());
    }

    private unsafe void HandleBalloon(nint hWnd)
    {
        if (nextBalloon is null) return;

        var iconData = new NOTIFYICONDATA
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = hWnd,
            guidItem = Id,
            hBalloonIcon = (nextBalloon.Icon is BalloonNotificationIcon.User) ? _hIco : nint.Zero,
            uFlags = PInvoke.NIF_INFO | PInvoke.NIF_GUID,
            dwInfoFlags = (uint)nextBalloon.Icon | (nextBalloon.NoSound ? PInvoke.NIIF_NOSOUND : 0) | (nextBalloon.Icon is BalloonNotificationIcon.User ? PInvoke.NIIF_LARGE_ICON : 0)
        };

        NativeString.WriteFixed(iconData.szInfoTitle, NOTIFYICONDATA.SZINFOTITLE_LENGTH, nextBalloon.Title);
        NativeString.WriteFixed(iconData.szInfo, NOTIFYICONDATA.SZINFO_LENGTH, nextBalloon.Message);

        nextBalloon = null;

        var success = PInvoke.Shell_NotifyIcon(PInvoke.NIM_MODIFY, ref iconData);
        if (!success) throw new Win32Exception(Marshal.GetLastPInvokeError());
    }
}