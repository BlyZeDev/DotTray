namespace DotTray.Internal;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using System;
using System.Runtime.Versioning;

[SupportedOSPlatform("windows")]
internal sealed class PopupMenuSession : IDisposable
{
    private static PopupMenuSession? activeSession;

    private readonly PopupMenuDismissHook _mouseHook;

    public NotifyIcon OwnerIcon { get; }
    public nint PopupWindowClassName { get; }
    public nint InstanceHandle { get; }
    public nint RootHWnd { get; }

    public event Action? Disposed;

    private PopupMenuSession(NotifyIcon ownerIcon, nint popupWindowClassName, nint instanceHandle, POINT mousePos)
    {
        OwnerIcon = ownerIcon;
        PopupWindowClassName = popupWindowClassName;
        InstanceHandle = instanceHandle;

        RootHWnd = PopupMenu.Show(this, mousePos);

        _mouseHook = new PopupMenuDismissHook(this);
        SetLeafHWnd(RootHWnd);
    }

    public void SetLeafHWnd(nint leafHWnd) => _mouseHook.LeafHWnd = leafHWnd;

    public void NotifyUpdate() => OwnerIcon.AttemptSessionRestart();

    public void Dispose()
    {
        if (activeSession != this) return;

        _mouseHook.Dispose();
        PInvoke.PostMessage(RootHWnd, PInvoke.WM_CLOSE, nint.Zero, nint.Zero);
        Disposed?.Invoke();

        activeSession = null;
    }

    public static PopupMenuSession Show(NotifyIcon ownerIcon, nint popupWindowClassName, nint instanceHandle, POINT mousePos)
    {
        activeSession?.Dispose();

        var session = new PopupMenuSession(ownerIcon, popupWindowClassName, instanceHandle, mousePos);

        return activeSession = session;
    }
}