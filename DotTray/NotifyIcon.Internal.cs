namespace DotTray;

using DotTray.Internal;
using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

public sealed partial class NotifyIcon
{
    internal void AttemptSessionRestart() => PInvoke.PostMessage(hWnd, PInvoke.WM_APP_TRAYICON_RESTART_SESSION, nint.Zero, nint.Zero);

    private unsafe nint WndProcFunc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case PInvoke.WM_APP_TRAYICON_CLICK: HandleClick(lParam); break;

            case PInvoke.WM_APP_TRAYICON_ICON: HandleIcon(hWnd, wParam); break;

            case PInvoke.WM_APP_TRAYICON_TOOLTIP: HandleToolTip(hWnd); break;

            case PInvoke.WM_APP_TRAYICON_VISIBILITY: HandleVisibility(hWnd); break;

            case PInvoke.WM_APP_TRAYICON_BALLOON: HandleBalloon(hWnd); break;

            case PInvoke.WM_APP_TRAYICON_RESTART_SESSION: HandleSessionRestartAttempt(); break;

            case PInvoke.WM_DESTROY: PInvoke.PostQuitMessage(0); return 0;
        }

        return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private void HandleClick(nint lParam)
    {
        if (MenuItems.IsEmpty) return;

        var clickedButton = lParam.ToInt32() switch
        {
            PInvoke.WM_LBUTTONUP => MouseButton.Left,
            PInvoke.WM_RBUTTONUP => MouseButton.Right,
            PInvoke.WM_MBUTTONUP => MouseButton.Middle,
            _ => MouseButton.None
        };

        if (clickedButton is not MouseButton.None && MouseButtons.HasFlag(clickedButton))
        {
            PInvoke.GetCursorPos(out lastMousePos);

            PopupShowing?.Invoke(clickedButton);
            popupMenuSession = PopupMenuSession.Show(this, _popupWindowClassName, instanceHandle, lastMousePos);
            popupMenuSession.Disposed += PopupDismissedCallback;
        }
    }

    private void PopupDismissedCallback()
    {
        popupMenuSession!.Disposed -= PopupDismissedCallback;
        popupMenuSession = null;
        PopupHiding?.Invoke();
    }

    private void HandleIcon(nint hWnd, nint wParam)
    {
        wParam = PInvoke.CopyIcon(wParam);

        PInvoke.DestroyIcon(baseIcoHandle);
        baseIcoHandle = wParam;

        var newShownIcon = CreateBadgedIcon(baseIcoHandle, Badge);

        var iconData = new NOTIFYICONDATA
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = hWnd,
            guidItem = Id,
            uFlags = PInvoke.NIF_ICON | PInvoke.NIF_GUID,
            uCallbackMessage = PInvoke.WM_APP_TRAYICON_CLICK,
            hIcon = newShownIcon
        };
        PInvoke.Shell_NotifyIcon(PInvoke.NIM_MODIFY, ref iconData);

        PInvoke.DestroyIcon(shownIcoHandle);
        shownIcoHandle = newShownIcon;
    }

    private unsafe void HandleToolTip(nint hWnd)
    {
        var iconData = new NOTIFYICONDATA
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = hWnd,
            guidItem = Id,
            uFlags = PInvoke.NIF_TIP | PInvoke.NIF_GUID
        };

        NativeString.WriteFixed(iconData.szTip, NOTIFYICONDATA.SZTIP_LENGTH, ToolTip);

        PInvoke.Shell_NotifyIcon(PInvoke.NIM_MODIFY, ref iconData);
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

        PInvoke.Shell_NotifyIcon(PInvoke.NIM_MODIFY, ref iconData);
    }

    private unsafe void HandleBalloon(nint hWnd)
    {
        if (nextBalloon is null) return;

        var iconData = new NOTIFYICONDATA
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = hWnd,
            guidItem = Id,
            hIcon = (nextBalloon.Icon is BalloonNotificationIcon.User) ? baseIcoHandle : nint.Zero,
            uFlags = PInvoke.NIF_INFO | PInvoke.NIF_GUID,
            dwInfoFlags = (uint)nextBalloon.Icon | (nextBalloon.NoSound ? PInvoke.NIIF_NOSOUND : 0)
        };

        NativeString.WriteFixed(iconData.szInfoTitle, NOTIFYICONDATA.SZINFOTITLE_LENGTH, nextBalloon.Title);
        NativeString.WriteFixed(iconData.szInfo, NOTIFYICONDATA.SZINFO_LENGTH, nextBalloon.Message);

        nextBalloon = null;

        PInvoke.Shell_NotifyIcon(PInvoke.NIM_MODIFY, ref iconData);
    }

    private void HandleSessionRestartAttempt()
    {
        if (popupMenuSession is null) return;

        popupMenuSession.Disposed -= PopupDismissedCallback;
        popupMenuSession = PopupMenuSession.Show(this, _popupWindowClassName, instanceHandle, lastMousePos);
        popupMenuSession.Disposed += PopupDismissedCallback;
    }

    private static NotifyIcon RunInternal(nint icoHandle, Action<MenuItem>? defaultMenuItemConfig, Action<SeparatorItem>? defaultSeparatorItemConfig, CancellationToken cancellationToken)
    {
        using (var manualLock = new ManualResetEventSlim(false))
        {
            var icon = new NotifyIcon(icoHandle, manualLock.Set, defaultMenuItemConfig, defaultSeparatorItemConfig, cancellationToken);

            manualLock.Wait(cancellationToken);

            return icon;
        }
    }

    private static async Task<NotifyIcon> RunInternalAsync(nint icoHandle, Action<MenuItem>? defaultMenuItemConfig, Action<SeparatorItem>? defaultSeparatorItemConfig, CancellationToken cancellationToken)
    {
        var manualLock = new AsyncManualResetEvent(false);

        var icon = new NotifyIcon(icoHandle, manualLock.Set, defaultMenuItemConfig, defaultSeparatorItemConfig, cancellationToken);

        await manualLock.WaitAsync(cancellationToken);

        return icon;
    }

    private static nint PrepareIconHandle(string icoPath)
    {
        if (!Path.GetExtension(icoPath).Equals(".ico", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("The path needs to point to an .ico file", nameof(icoPath));
        if (!File.Exists(icoPath)) throw new FileNotFoundException("The .ico file could not be found", icoPath);

        var handle = PInvoke.LoadImage(nint.Zero, icoPath, PInvoke.IMAGE_ICON, 0, 0, PInvoke.LR_LOADFROMFILE | PInvoke.LR_DEFAULTSIZE);
        return handle == nint.Zero ? throw new FileLoadException("The .ico file could not be loaded", icoPath) : handle;
    }

    private static nint CreateBadgedIcon(nint baseIcoHandle, NotifyIconBadge? badge)
    {
        if (badge is null) return PInvoke.CopyIcon(baseIcoHandle);

        PInvoke.GetIconInfo(baseIcoHandle, out var iconInfo);

        try
        {
            PInvoke.GetObject(iconInfo.hbmColor, Marshal.SizeOf<BITMAP>(), out var bitmap);

            var width = bitmap.bmWidth;
            var height = bitmap.bmHeight;

            PInvoke.GdipCreateBitmapFromScan0(width, height, 0, PInvoke.Format32bppArgb, nint.Zero, out nint bitmapHandle);
            PInvoke.GdipGetImageGraphicsContext(bitmapHandle, out nint graphicsHandle);
            PInvoke.GdipSetSmoothingMode(graphicsHandle, PInvoke.SmoothingModeAntiAlias8x8);

            PInvoke.GdipCreateBitmapFromHBITMAP(iconInfo.hbmColor, nint.Zero, out nint iconBitmapHandle);
            PInvoke.GdipDrawImageRect(graphicsHandle, iconBitmapHandle, 0f, 0f, width, height);
            PInvoke.GdipDisposeImage(iconBitmapHandle);

            var badgeSize = MathF.Min(width, height) * 0.5f;
            var halfBadge = badgeSize * 0.5f;

            var centerX = badge.Position switch
            {
                NotifyIconBadgePosition.TopLeft or NotifyIconBadgePosition.BottomLeft => halfBadge,
                _ => width - halfBadge
            };

            var centerY = badge.Position switch
            {
                NotifyIconBadgePosition.TopLeft or NotifyIconBadgePosition.TopRight => halfBadge,
                _ => height - halfBadge
            };

            var rect = new RECTF
            {
                X = centerX - badgeSize * 0.5f,
                Y = centerY - badgeSize * 0.5f,
                Width = badgeSize,
                Height = badgeSize
            };

            PInvoke.GdipCreatePath(0, out nint path);

            var radius = Math.Clamp(badge.BorderRadius, 0f, 1f) * (badgeSize * 0.5f);
            if (radius >= badgeSize * 0.5f)
            {
                PInvoke.GdipAddPathEllipse(path, rect.X, rect.Y, rect.Width, rect.Height);
            }
            else if (radius <= 0f)
            {
                PInvoke.GdipAddPathRectangle(path, rect.X, rect.Y, rect.Width, rect.Height);
            }
            else
            {
                var rX = Math.Min(radius, rect.Width * 0.5f);
                var rY = Math.Min(radius, rect.Height * 0.5f);

                PInvoke.GdipAddPathArc(path, rect.X, rect.Y, rX * 2, rY * 2, 180, 90);
                PInvoke.GdipAddPathArc(path, rect.X + rect.Width - rX * 2, rect.Y, rX * 2, rY * 2, 270, 90);
                PInvoke.GdipAddPathArc(path, rect.X + rect.Width - rX * 2, rect.Y + rect.Height - rY * 2, rX * 2, rY * 2, 0, 90);
                PInvoke.GdipAddPathArc(path, rect.X, rect.Y + rect.Height - rY * 2, rX * 2, rY * 2, 90, 90);
                PInvoke.GdipClosePathFigure(path);
            }

            PInvoke.GdipCreateSolidFill(badge.BackgroundColor.ToGdiPlus(), out var bgBrush);
            PInvoke.GdipFillPath(graphicsHandle, bgBrush, path);
            PInvoke.GdipDeleteBrush(bgBrush);

            PInvoke.GdipDeletePath(path);

            PInvoke.GdipCreateHBITMAPFromBitmap(bitmapHandle, out var bitmapReturnHandle, 0);
            var createIconInfo = new ICONINFO
            {
                fIcon = 1,
                hbmColor = bitmapReturnHandle,
                hbmMask = bitmapReturnHandle,
            };

            var resultHandle = PInvoke.CreateIconIndirect(ref createIconInfo);

            PInvoke.DeleteObject(bitmapReturnHandle);
            PInvoke.GdipDeleteGraphics(graphicsHandle);
            PInvoke.GdipDisposeImage(bitmapHandle);

            return resultHandle;
        }
        finally
        {
            PInvoke.DeleteObject(iconInfo.hbmColor);
            PInvoke.DeleteObject(iconInfo.hbmMask);
        }
    }
}