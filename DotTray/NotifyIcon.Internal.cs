namespace DotTray;

using DotTray.Internal;
using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

public sealed partial class NotifyIcon
{
    private void SetIcon(nint icoHandle, bool needsIcoDestroy) => PInvoke.PostMessage(hWnd, PInvoke.WM_APP_TRAYICON_ICON, icoHandle, needsIcoDestroy ? 1 : 0);

    private unsafe nint WndProcFunc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case PInvoke.WM_APP_TRAYICON: HandleClick(lParam); break;

            case PInvoke.WM_APP_TRAYICON_ICON: HandleIcon(hWnd, wParam, lParam); break;

            case PInvoke.WM_APP_TRAYICON_TOOLTIP: HandleToolTip(hWnd); break;

            case PInvoke.WM_APP_TRAYICON_BALLOON: HandleBalloon(hWnd); break;

            case PInvoke.WM_DESTROY: PInvoke.PostQuitMessage(0); return 0;
        }

        return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private void HandleClick(nint lParam)
    {
        var clickedButton = lParam.ToInt32() switch
        {
            PInvoke.WM_LBUTTONUP => MouseButton.Left,
            PInvoke.WM_RBUTTONUP => MouseButton.Right,
            PInvoke.WM_MBUTTONUP => MouseButton.Middle,
            _ => MouseButton.None
        };

        if (clickedButton is not MouseButton.None && MouseButtons.HasFlag(clickedButton))
        {
            PInvoke.GetCursorPos(out var mousePos);

            MenuShowing?.Invoke(clickedButton);
            popupMenu = PopupMenuSession.Show(this, _popupWindowClassName, instanceHandle, mousePos);
            popupMenu.Disposed += PopupDismissedCallback;
        }
    }

    private void PopupDismissedCallback()
    {
        popupMenu!.Disposed -= PopupDismissedCallback;
        popupMenu = null;
        MenuHiding?.Invoke();
    }

    private void HandleIcon(nint hWnd, nint wParam, nint lParam)
    {
        var iconData = new NOTIFYICONDATA
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = hWnd,
            guidItem = _trayId,
            uFlags = PInvoke.NIF_ICON | PInvoke.NIF_GUID,
            uCallbackMessage = PInvoke.WM_APP_TRAYICON,
            hIcon = wParam
        };
        PInvoke.Shell_NotifyIcon(PInvoke.NIM_MODIFY, ref iconData);

        if (needsIcoDestroy) PInvoke.DestroyIcon(icoHandle);

        icoHandle = wParam;
        needsIcoDestroy = lParam != 0;
    }

    private void HandleToolTip(nint hWnd)
    {
        var iconData = new NOTIFYICONDATA
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = hWnd,
            guidItem = _trayId,
            uFlags = PInvoke.NIF_TIP | PInvoke.NIF_GUID,
            szTip = ToolTip
        };

        PInvoke.Shell_NotifyIcon(PInvoke.NIM_MODIFY, ref iconData);
    }

    private void HandleBalloon(nint hWnd)
    {
        if (nextBalloon is null) return;

        var iconData = new NOTIFYICONDATA
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = hWnd,
            guidItem = _trayId,
            hIcon = (nextBalloon.Icon is BalloonNotificationIcon.User) ? icoHandle : nint.Zero,
            uFlags = PInvoke.NIF_INFO | PInvoke.NIF_GUID,
            dwInfoFlags = (uint)nextBalloon.Icon | (nextBalloon.NoSound ? PInvoke.NIIF_NOSOUND : 0),
            szInfoTitle = nextBalloon.Title,
            szInfo = nextBalloon.Message
        };

        nextBalloon = null;

        PInvoke.Shell_NotifyIcon(PInvoke.NIM_MODIFY, ref iconData);
    }

    private static NotifyIcon Run(nint iconHandle, bool needIconDestroy, CancellationToken cancellationToken)
    {
        using (var manualLock = new ManualResetEventSlim(false))
        {
            var icon = new NotifyIcon(iconHandle, needIconDestroy, manualLock.Set, cancellationToken);

            manualLock.Wait(cancellationToken);

            return icon;
        }
    }

    private static async Task<NotifyIcon> RunAsync(nint iconHandle, bool needIconDestroy, CancellationToken cancellationToken)
    {
        var manualLock = new AsyncManualResetEvent(false);

        var icon = new NotifyIcon(iconHandle, needIconDestroy, manualLock.Set, cancellationToken);

        await manualLock.WaitAsync(cancellationToken);

        return icon;
    }

    private static nint PrepareIconHandle(string icoPath)
    {
        if (!Path.GetExtension(icoPath).Equals(".ico", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("The path needs to point to an .ico file", nameof(icoPath));
        if (!File.Exists(icoPath)) throw new FileNotFoundException("The .ico file could not be found", icoPath);

        var handle = PInvoke.LoadImage(nint.Zero, icoPath, PInvoke.IMAGE_ICON, 16, 16, PInvoke.LR_LOADFROMFILE);
        return handle == nint.Zero ? throw new FileLoadException("The .ico file could not be loaded", icoPath) : handle;
    }
}