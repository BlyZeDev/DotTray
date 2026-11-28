namespace DotTray.Direct.Internal.Popup;

using DotTray.Direct.Context;
using DotTray.Direct.Internal.Rendering;
using DotTray.Direct.Items;
using DotTray.Direct.Primitives;
using System.Runtime.InteropServices;
using OpenSubmenu = (PopupWindow? Window, DotTray.Direct.Items.MenuItemBase? Owner);

internal sealed class PopupWindow
{
    private const float BaseDpi = 96f;
    private const nint SubmenuTimerId = 1;

    internal const uint WM_APP_LAYOUT = PInvoke.WM_APP + 0x2000;

    private readonly PopupSession _session;
    private readonly MenuItemCollection _items;
    private readonly Rect _anchor;
    private readonly bool _selectFirstItem;
    private readonly float _dpi;
    private readonly float _scale;
    private readonly DirectSurface _surface;

    private GCHandle handle;
    private PopupLayout layout;
    private MenuItemBase? hotItem;
    private OpenSubmenu openSubmenu;
    private bool submenuTimerActive;
    private Pos lastPoint;
    private int layoutRequested;
    private bool destroyed;

    public nint HWnd { get; }

    public PopupWindow? Parent { get; }

    public Rect ScreenBounds { get; private set; }

    public PopupWindow(PopupSession session, PopupWindow? parent, MenuItemCollection items, Rect anchorScreenRect, bool selectFirstItem)
    {
        _session = session;
        Parent = parent;
        _items = items;
        _anchor = anchorScreenRect;
        _selectFirstItem = selectFirstItem;

        layout = PopupLayout.Empty;

        foreach (var item in _items.GetSnapshot())
        {
            item.Initialize();
        }

        handle = GCHandle.Alloc(this);

        try
        {
            var exStyle = PInvoke.WS_EX_TOOLWINDOW | PInvoke.WS_EX_TOPMOST | PInvoke.WS_EX_NOREDIRECTIONBITMAP;
            if (parent is not null) exStyle |= PInvoke.WS_EX_NOACTIVATE;

            HWnd = PopupWindowClass.CreatePopupWindow(GCHandle.ToIntPtr(handle), exStyle, anchorScreenRect.X, anchorScreenRect.Y, parent?.HWnd ?? nint.Zero);
        }
        catch
        {
            handle.Free();
            CleanupItems();
            throw;
        }

        try
        {
            _dpi = PInvoke.GetDpiForWindow(HWnd);
            _scale = _dpi / BaseDpi;

            var cornerRadius = PInvoke.DWMWCP_ROUND;
            PInvoke.DwmSetWindowAttribute(HWnd, PInvoke.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerRadius, sizeof(uint));

            var borderColor = PInvoke.DWMWA_COLOR_NONE;
            PInvoke.DwmSetWindowAttribute(HWnd, PInvoke.DWMWA_BORDER_COLOR, ref borderColor, sizeof(uint));

            _surface = new DirectSurface(session.Device, HWnd, 1, 1, _dpi);
            var bounds = ComputeLayout();
            _surface.Resize(bounds.Width, bounds.Height, _dpi);

            _items.Updated += RequestLayout;

            ApplyWindowPos(bounds);
        }
        catch
        {
            _items.Updated -= RequestLayout;
            PInvoke.DestroyWindow(HWnd);
            throw;
        }
    }

    public void Show(bool activate)
    {
        PInvoke.ShowWindow(HWnd, activate ? PInvoke.SW_SHOW : PInvoke.SW_SHOWNOACTIVATE);
        PInvoke.InvalidateRect(HWnd, nint.Zero, false);

        if (_selectFirstItem) SelectFirstEnabledItem();
    }

    public void Close() => PInvoke.PostMessage(HWnd, PInvoke.WM_CLOSE, nint.Zero, nint.Zero);

    public void RequestLayout()
    {
        if (Interlocked.Exchange(ref layoutRequested, 1) != 0) return;

        PInvoke.PostMessage(HWnd, WM_APP_LAYOUT, nint.Zero, nint.Zero);
    }

    public void OnChildClosed(PopupWindow child)
    {
        if (!ReferenceEquals(openSubmenu.Window, child)) return;

        openSubmenu = (null, null);
    }

    public void PointerMove(Pos screenPixel)
    {
        var pixel = ToClientPixel(screenPixel);
        var point = ToDipPoint(pixel);
        var moved = point.X != lastPoint.X || point.Y != lastPoint.Y;
        lastPoint = point;

        var hit = HitItem(pixel);

        SetHotItem(hit, b => RelativePosition(b, lastPoint), ItemInteractionType.MouseEnter, ItemInteractionType.MouseLeave);

        if (hit is not null && moved)
        {
            hit.RaiseInteraction(new ItemInteractedEventArgs
            {
                Type = ItemInteractionType.MouseMove,
                Position = RelativePosition(layout.BoundsOf(hit), point)
            });
        }
    }

    public void PointerLeft()
    {
        if (openSubmenu.Owner is not null)
        {
            SetHotItem(openSubmenu.Owner, b => RelativePosition(b, lastPoint), ItemInteractionType.MouseEnter, ItemInteractionType.MouseLeave);
            KillSubmenuTimer();
            return;
        }

        SetHotItem(null, b => RelativePosition(b, lastPoint), ItemInteractionType.MouseEnter, ItemInteractionType.MouseLeave);
    }

    public void PointerButton(Pos screenPixel, ItemInteractionType type)
    {
        var pixel = ToClientPixel(screenPixel);
        var point = ToDipPoint(pixel);

        var item = HitItem(pixel);
        if (item is null) return;

        Interact(item, RelativePosition(layout.BoundsOf(item), point), type, false);
    }

    public void HandleKeyDown(int vkCode)
    {
        switch (vkCode)
        {
            case PInvoke.VK_DOWN: MoveHot(PopupNavigation.Step(layout.Items, layout.IndexOf(hotItem), +1)); break;
            case PInvoke.VK_UP: MoveHot(PopupNavigation.Step(layout.Items, layout.IndexOf(hotItem), -1)); break;

            case PInvoke.VK_HOME: MoveHot(PopupNavigation.First(layout.Items)); break;
            case PInvoke.VK_END: MoveHot(PopupNavigation.Last(layout.Items)); break;

            case PInvoke.VK_RIGHT: OpenHotSubmenu(); break;
            case PInvoke.VK_RETURN or PInvoke.VK_SPACE: ActivateHot(); break;

            case PInvoke.VK_LEFT: _session.NavigateBack(); break;
            case PInvoke.VK_ESCAPE: _session.CloseFromEscape(); break;
        }
    }

    public nint HandleMessage(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case PInvoke.WM_NCACTIVATE: return 1;
            case PInvoke.WM_SETCURSOR: PInvoke.SetCursor(PopupWindowClass.ArrowCursor); return 1;
            case WM_APP_LAYOUT: return HandleLayoutRequest();
            case PInvoke.WM_TIMER: return HandleTimer(wParam);
            case PInvoke.WM_ERASEBKGND: return 1;
            case PInvoke.WM_PAINT: return HandlePaint(hWnd);

            case PInvoke.WM_MOUSEMOVE or PInvoke.WM_LBUTTONDOWN or PInvoke.WM_LBUTTONUP or PInvoke.WM_RBUTTONDOWN or PInvoke.WM_RBUTTONUP or PInvoke.WM_MBUTTONDOWN:
                _session.OnMouse(this, msg, lParam);
                return 0;

            case PInvoke.WM_KEYDOWN: _session.OnKeyDown((int)wParam); return 0;

            case PInvoke.WM_SYSKEYDOWN when (int)wParam == PInvoke.VK_MENU: _session.Dismiss(); return 0;

            case PInvoke.WM_ACTIVATE when (int)(wParam & 0xFFFF) == PInvoke.WA_INACTIVE: _session.Dismiss(); return 0;

            case PInvoke.WM_CAPTURECHANGED: _session.OnCaptureChanged(this, lParam); return 0;

            case PInvoke.WM_DISPLAYCHANGE or PInvoke.WM_DPICHANGED or PInvoke.WM_CANCELMODE: return Dismiss(hWnd, msg, wParam, lParam);
            case PInvoke.WM_SETTINGCHANGE when wParam == PInvoke.SPI_SETWORKAREA: return Dismiss(hWnd, msg, wParam, lParam);

            case PInvoke.WM_CLOSE: PInvoke.DestroyWindow(hWnd); return 0;
            case PInvoke.WM_DESTROY: return HandleDestroy();
            case PInvoke.WM_NCDESTROY: return HandleNcDestroy(hWnd, msg, wParam, lParam);
        }

        return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private Rect ComputeLayout()
    {
        layout = PopupLayout.Measure(_items.GetSnapshot(), new MeasuringContext(_surface));
        DropStaleHotItem();

        var pixelSize = new Dim(ToPixels(layout.Size.Width), ToPixels(layout.Size.Height));
        var position = CalculatePosition(pixelSize);

        return new Rect(position.X, position.Y, pixelSize.Width, pixelSize.Height);
    }

    private Pos CalculatePosition(Dim pixelSize)
    {
        var workArea = PopupPlacement.GetWorkArea(_anchor);

        return Parent is null
            ? PopupPlacement.ResolveRoot(_anchor, pixelSize.Width, pixelSize.Height, workArea)
            : PopupPlacement.ResolveSubmenu(_anchor, pixelSize.Width, pixelSize.Height, workArea, _session.GetOpenWindowRects(this));
    }

    private void ApplyWindowPos(Rect bounds)
    {
        ScreenBounds = bounds;
        PInvoke.SetWindowPos(HWnd, nint.Zero, bounds.X, bounds.Y, bounds.Width, bounds.Height, PInvoke.SWP_NOZORDER | PInvoke.SWP_NOACTIVATE | PInvoke.SWP_NOCOPYBITS);
    }

    private void DrawWindow(DrawingContext context)
    {
        context.FillRect(new RectF(0, 0, context.WindowSize.Width, context.WindowSize.Height), _session.Handler.Background ?? DirectDefaultStyles.PopupWindowBackground);

        foreach (var entry in layout.Items)
        {
            using (context.Clip(entry.Bounds))
            {
                context.ItemBounds = entry.Bounds;
                entry.Item.Draw(context);
            }
        }
    }

    private MenuItemBase? HitItem(Pos clientPixel)
    {
        var index = layout.HitTest(clientPixel, _scale);
        return index >= 0 ? layout.Items[index].Item : null;
    }

    private void DropStaleHotItem()
    {
        if (hotItem is null || layout.IndexOf(hotItem) >= 0) return;

        CloseOpenSubmenu();
        hotItem = null;
    }

    private nint HandleLayoutRequest()
    {
        Volatile.Write(ref layoutRequested, 0);

        var bounds = ComputeLayout();

        _surface.Resize(bounds.Width, bounds.Height, _dpi);

        if (_surface.IsLost)
        {
            _session.Dismiss();
            return 0;
        }

        ApplyWindowPos(bounds);
        PInvoke.InvalidateRect(HWnd, nint.Zero, false);

        return 0;
    }

    private nint HandlePaint(nint hWnd)
    {
        PInvoke.BeginPaint(hWnd, out var paint);

        try
        {
            if (_surface.IsLost)
            {
                _session.Dismiss();
                return 0;
            }

            var context = new DrawingContext(_surface, layout.Size);

            using (_surface.BeginDraw())
            {
                DrawWindow(context);
            }

            if (_surface.IsLost) _session.Dismiss();
        }
        finally
        {
            PInvoke.EndPaint(hWnd, ref paint);
        }

        return 0;
    }

    private nint HandleTimer(nint wParam)
    {
        if (wParam != SubmenuTimerId) return 0;

        KillSubmenuTimer();

        if (openSubmenu.Owner is not null && !ReferenceEquals(openSubmenu.Owner, hotItem)) CloseOpenSubmenu();

        if (hotItem?.CanOpenSubmenu(ItemInteractionType.MouseEnter) ?? false) OpenSubmenu(hotItem, false);

        return 0;
    }

    private nint HandleDestroy()
    {
        if (destroyed) return 0;
        destroyed = true;

        KillSubmenuTimer();
        _items.Updated -= RequestLayout;

        try
        {
            CleanupItems();
        }
        finally
        {
            _surface?.Dispose();
            _session.OnWindowDestroyed(this);
        }

        return 0;
    }

    private nint HandleNcDestroy(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        PInvoke.SetWindowLongPtr(hWnd, PInvoke.GWLP_USERDATA, nint.Zero);
        if (handle.IsAllocated) handle.Free();

        return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private nint Dismiss(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        _session.Dismiss();
        return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private void CleanupItems()
    {
        foreach (var item in _items.GetSnapshot())
        {
            try
            {
                item.Cleanup();
            }
            catch (Exception ex)
            {
                PopupThread.Fail(ex);
            }
        }
    }

    private void SetHotItem(MenuItemBase? newHot, Func<Rect, Pos> positionFor, ItemInteractionType enterType, ItemInteractionType leaveType)
    {
        if (ReferenceEquals(newHot, hotItem)) return;

        KillSubmenuTimer();

        hotItem?.RaiseInteraction(new ItemInteractedEventArgs
        {
            Type = leaveType,
            Position = positionFor(layout.BoundsOf(hotItem))
        });

        hotItem = newHot;

        if (!ReferenceEquals(newHot, openSubmenu.Owner))
        {
            if (openSubmenu.Owner is not null && enterType is ItemInteractionType.MouseEnter) StartSubmenuTimer();
            else CloseOpenSubmenu();
        }

        if (newHot is not null)
        {
            var args = new ItemInteractedEventArgs
            {
                Type = enterType,
                Position = positionFor(layout.BoundsOf(newHot))
            };
            newHot.RaiseInteraction(args);

            if (newHot.CanOpenSubmenu(enterType)) StartSubmenuTimer();
        }

        PInvoke.InvalidateRect(HWnd, nint.Zero, false);
    }

    private void Interact(MenuItemBase item, Pos position, ItemInteractionType type, bool selectFirstOnSubmenu)
    {
        var args = new ItemInteractedEventArgs
        {
            Type = type,
            Position = position
        };
        item.RaiseInteraction(args);

        if (item.CanOpenSubmenu(args.Type))
        {
            OpenSubmenu(item, selectFirstOnSubmenu);
            return;
        }

        if (!args.KeepMenuOpen && type is ItemInteractionType.MouseLeftUp or ItemInteractionType.KeyboardActivate)
        {
            _session.Dismiss();
        }
    }

    private void MoveHot(int index)
    {
        if (index < 0) return;

        SetHotItem(layout.Items[index].Item, CenterOf, ItemInteractionType.KeyboardFocus, ItemInteractionType.KeyboardBlur);
    }

    private void OpenHotSubmenu()
    {
        if (hotItem is null) return;
        if (!hotItem.CanOpenSubmenu(ItemInteractionType.KeyboardActivate)) return;

        KillSubmenuTimer();
        OpenSubmenu(hotItem, true);
    }

    private void ActivateHot()
    {
        if (hotItem is null) return;

        Interact(hotItem, CenterOf(layout.BoundsOf(hotItem)), ItemInteractionType.KeyboardActivate, true);
    }

    private void SelectFirstEnabledItem() => MoveHot(PopupNavigation.First(layout.Items));

    private void StartSubmenuTimer()
    {
        PInvoke.SystemParametersInfo(PInvoke.SPI_GETMENUSHOWDELAY, 0, out var delay, 0);
        PInvoke.SetTimer(HWnd, SubmenuTimerId, delay, nint.Zero);
        submenuTimerActive = true;
    }

    private void KillSubmenuTimer()
    {
        if (!submenuTimerActive) return;

        PInvoke.KillTimer(HWnd, SubmenuTimerId);
        submenuTimerActive = false;
    }

    private void OpenSubmenu(MenuItemBase submenu, bool selectFirstItem)
    {
        if (ReferenceEquals(openSubmenu.Owner, submenu)) return;

        var child = _session.OpenChild(this, submenu.SubmenuItems, ToScreenRect(layout.BoundsOf(submenu)), selectFirstItem);
        openSubmenu = (child, submenu);
    }

    private void CloseOpenSubmenu()
    {
        if (openSubmenu.Owner is null) return;

        _session.CloseChildrenOf(this);
        openSubmenu = (null, null);
    }

    private Rect ToScreenRect(Rect localDips)
    {
        var left = ToPixels(localDips.Left);
        var top = ToPixels(localDips.Top);

        return new Rect(ScreenBounds.X + left, ScreenBounds.Y + top, ToPixels(localDips.Right) - left, ToPixels(localDips.Bottom) - top);
    }

    private Pos ToClientPixel(Pos screenPixel) => new Pos(screenPixel.X - ScreenBounds.X, screenPixel.Y - ScreenBounds.Y);

    private int ToPixels(int dips) => PixelGrid.ToPixel(dips, _scale);

    private Pos ToDipPoint(Pos pixel) => new Pos(PixelGrid.ToDip(pixel.X, _scale), PixelGrid.ToDip(pixel.Y, _scale));

    private static Pos RelativePosition(Rect bounds, Pos point) => new Pos(point.X - bounds.X, point.Y - bounds.Y);

    private static Pos CenterOf(Rect bounds) => new Pos(bounds.Width / 2, bounds.Height / 2);
}