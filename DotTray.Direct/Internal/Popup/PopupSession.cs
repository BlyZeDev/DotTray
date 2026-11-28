namespace DotTray.Direct.Internal.Popup;

using DotTray.Direct.Internal.Rendering;
using DotTray.Direct.Primitives;

internal sealed class PopupSession
{
    private readonly PopupThread _owner;
    private readonly List<PopupWindow> _windows = [];

    private PopupWindow? hoverWindow;
    private int active;
    private bool captured;
    private bool announced;
    private nint rootHWnd;
    private nint ownerHWnd;

    public DirectPopupMenuHandler Handler => _owner.Handler;

    public DirectDevice Device => _owner.Device;

    public bool IsActive => Volatile.Read(ref active) != 0;

    public PopupSession(PopupThread owner) => _owner = owner;

    public bool TryOpen(Rect anchor, nint ownerHWnd)
    {
        this.ownerHWnd = ownerHWnd;

        Volatile.Write(ref active, 1);

        var root = new PopupWindow(this, null, Handler.MenuItems, anchor, false);
        _windows.Add(root);

        root.Show(true);

        PInvoke.SetForegroundWindow(root.HWnd);
        if (PInvoke.GetForegroundWindow() != root.HWnd)
        {
            Abort();
            return false;
        }

        PInvoke.SetCapture(root.HWnd);
        captured = true;

        PInvoke.SetCursor(PopupWindowClass.ArrowCursor);

        rootHWnd = root.HWnd;
        announced = true;
        Announce(PInvoke.EVENT_SYSTEM_MENUSTART, rootHWnd);
        Announce(PInvoke.EVENT_SYSTEM_MENUPOPUPSTART, rootHWnd);

        return true;
    }

    public PopupWindow OpenChild(PopupWindow owner, MenuItemCollection items, Rect anchorScreenRect, bool selectFirstItem)
    {
        CloseChildrenOf(owner);

        var child = new PopupWindow(this, owner, items, anchorScreenRect, selectFirstItem);
        _windows.Add(child);

        child.Show(false);
        Announce(PInvoke.EVENT_SYSTEM_MENUPOPUPSTART, child.HWnd);

        return child;
    }

    public void CloseChildrenOf(PopupWindow window)
    {
        if (!_windows.Contains(window)) return;

        while (_windows.Count > 0 && !ReferenceEquals(_windows[^1], window))
        {
            CloseLeaf();
        }
    }

    public void NavigateBack()
    {
        if (_windows.Count <= 1) return;

        CloseLeaf();
    }

    public void CloseFromEscape()
    {
        if (_windows.Count <= 1) Dismiss();
        else CloseLeaf();
    }

    public void Dismiss()
    {
        if (!Deactivate(true)) return;

        if (_windows.Count > 0) _windows[0].Close();
    }

    public void Abort()
    {
        Deactivate(false);

        if (_windows.Count > 0) PInvoke.DestroyWindow(_windows[0].HWnd);
        else _owner.OnSessionEnded(this);
    }

    public void RequestRelayoutAll()
    {
        foreach (var window in _windows.ToArray())
        {
            window.RequestLayout();
        }
    }

    public IReadOnlyCollection<Rect> GetOpenWindowRects(PopupWindow exclude)
    {
        var rects = new List<Rect>(_windows.Count);

        foreach (var window in _windows)
        {
            if (!ReferenceEquals(window, exclude)) rects.Add(window.ScreenBounds);
        }

        return rects;
    }

    public void OnMouse(PopupWindow source, uint msg, nint lParam)
    {
        if (!IsActive) return;

        var value = (int)lParam.ToInt64();
        var x = unchecked((short)(value & 0xFFFF));
        var y = unchecked((short)((value >> 16) & 0xFFFF));

        var screen = new Pos(source.ScreenBounds.X + x, source.ScreenBounds.Y + y);
        var target = WindowAt(screen);

        switch (msg)
        {
            case PInvoke.WM_MOUSEMOVE:
                UpdateHover(target);
                target?.PointerMove(screen);
                break;

            case PInvoke.WM_LBUTTONDOWN:
                if (target is null) Dismiss();
                else target.PointerButton(screen, ItemInteractionType.MouseLeftDown);
                break;

            case PInvoke.WM_LBUTTONUP:
                target?.PointerButton(screen, ItemInteractionType.MouseLeftUp);
                break;

            case PInvoke.WM_RBUTTONDOWN:
                if (target is null) Dismiss();
                else target.PointerButton(screen, ItemInteractionType.MouseRightDown);
                break;

            case PInvoke.WM_RBUTTONUP:
                target?.PointerButton(screen, ItemInteractionType.MouseRightUp);
                break;

            case PInvoke.WM_MBUTTONDOWN:
                if (target is null) Dismiss();
                break;
        }
    }

    public void OnKeyDown(int vkCode)
    {
        if (!IsActive || _windows.Count == 0) return;

        _windows[^1].HandleKeyDown(vkCode);
    }

    public void OnCaptureChanged(PopupWindow source, nint newCapture)
    {
        if (!IsActive || _windows.Count == 0) return;
        if (!ReferenceEquals(source, _windows[0]) || newCapture == source.HWnd) return;

        Dismiss();
    }

    public void OnWindowDestroyed(PopupWindow window)
    {
        _windows.Remove(window);

        if (ReferenceEquals(hoverWindow, window)) hoverWindow = null;
        if (window.Parent is not null) return;

        Deactivate(false);
        _owner.OnSessionEnded(this);
    }

    private void CloseLeaf()
    {
        var leaf = _windows[^1];
        _windows.RemoveAt(_windows.Count - 1);

        if (ReferenceEquals(hoverWindow, leaf)) hoverWindow = null;

        leaf.Parent?.OnChildClosed(leaf);
        Announce(PInvoke.EVENT_SYSTEM_MENUPOPUPEND, leaf.HWnd);
        leaf.Close();
    }

    private PopupWindow? WindowAt(Pos screen)
    {
        for (var i = _windows.Count - 1; i >= 0; i--)
        {
            var bounds = _windows[i].ScreenBounds;

            if (screen.X >= bounds.Left && screen.X < bounds.Right && screen.Y >= bounds.Top && screen.Y < bounds.Bottom) return _windows[i];
        }

        return null;
    }

    private void UpdateHover(PopupWindow? target)
    {
        if (ReferenceEquals(target, hoverWindow)) return;

        var prev = hoverWindow;
        hoverWindow = target;

        prev?.PointerLeft();
    }

    private bool Deactivate(bool handOffForeground)
    {
        if (Interlocked.Exchange(ref active, 0) == 0) return false;

        if (captured)
        {
            captured = false;
            PInvoke.ReleaseCapture();
        }

        if (handOffForeground && ownerHWnd != nint.Zero && OwnsForeground()) PInvoke.SetForegroundWindow(ownerHWnd);

        if (announced)
        {
            announced = false;

            for (var i = _windows.Count - 1; i >= 0; i--)
            {
                Announce(PInvoke.EVENT_SYSTEM_MENUPOPUPEND, _windows[i].HWnd);
            }

            if (_windows.Count == 0) Announce(PInvoke.EVENT_SYSTEM_MENUPOPUPEND, rootHWnd);

            Announce(PInvoke.EVENT_SYSTEM_MENUEND, rootHWnd);
        }

        return true;
    }

    private bool OwnsForeground()
    {
        var foreground = PInvoke.GetForegroundWindow();
        if (foreground == nint.Zero) return false;
        if (foreground == rootHWnd) return true;

        foreach (var window in _windows)
        {
            if (window.HWnd == foreground) return true;
        }

        return false;
    }

    private static void Announce(uint winEvent, nint hWnd) => PInvoke.NotifyWinEvent(winEvent, hWnd, PInvoke.OBJID_CLIENT, PInvoke.CHILDID_SELF);
}