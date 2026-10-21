namespace DotTray.Direct.Internal;

using DotTray.Direct.Context;
using DotTray.Direct.Internal.Models;
using DotTray.Direct.Internal.Rendering;
using DotTray.Direct.Items;
using DotTray.Direct.Primitives;
using System.Runtime.InteropServices;

internal sealed class PopupMenu
{
    private const float BaseDpi = 96f;
    private const uint SubmenuHoverDelayMs = 350;
    private const nint SubmenuTimerId = 1;

    internal const uint WM_APP_POPUP_CALCWND = PInvoke.WM_APP + 0x2000;
    internal const uint WM_APP_POPUP_KEYDOWN = PInvoke.WM_APP + 0x2001;

    private readonly PopupMenuTree _tree;
    private readonly MenuItemCollection _items;
    private readonly bool _isRoot;
    private readonly Rect _anchor;
    private readonly float _dpi;
    private readonly float _scale;
    private readonly PInvoke.WndProc _wndProc;
    private readonly DirectSurface _surface;

    private WindowLayout windowLayout = new(default, []);
    private MenuItemBase? hotItem;
    private MenuItemBase? openSubmenuOwner;
    private bool submenuTimerActive;
    private bool tracking;
    private Pos lastPoint;

    internal nint HWnd { get; }

    internal nint OwnerHWnd { get; }

    internal PopupMenu(PopupMenuTree tree, nint ownerHWnd, MenuItemCollection items, Rect? anchorScreenRect, bool selectFirstItem)
    {
        _tree = tree;
        _items = items;
        _isRoot = anchorScreenRect is null;
        _anchor = anchorScreenRect ?? PopupPlacement.GetCursorAnchor();

        OwnerHWnd = ownerHWnd;

        foreach (var item in _items.GetSnapshot()) item.Initialize();

        PopupWindowHelper.EnsureRegistered(out var hInstance, out var hClassName);

        // Created tiny at the anchor so the window already lives on the target monitor and reports that monitor's DPI
        HWnd = PInvoke.CreateWindowEx(
            PInvoke.WS_EX_NOACTIVATE | PInvoke.WS_EX_TOOLWINDOW | PInvoke.WS_EX_TOPMOST | PInvoke.WS_EX_NOREDIRECTIONBITMAP,
            hClassName, nint.Zero,
            PInvoke.WS_CLIPCHILDREN | PInvoke.WS_CLIPSIBLINGS | PInvoke.WS_POPUP,
            _anchor.X, _anchor.Y, 1, 1,
            ownerHWnd, nint.Zero, hInstance, nint.Zero);

        try
        {
            _dpi = PInvoke.GetDpiForWindow(HWnd);
            _scale = _dpi / BaseDpi;

            var cornerRadius = PInvoke.DWMWCP_ROUND;
            PInvoke.DwmSetWindowAttribute(HWnd, PInvoke.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerRadius, sizeof(uint));

            var borderColor = PInvoke.DWMWA_COLOR_NONE;
            PInvoke.DwmSetWindowAttribute(HWnd, PInvoke.DWMWA_BORDER_COLOR, ref borderColor, sizeof(uint));

            var (pixelSize, position) = ComputeLayout();
            _surface = new DirectSurface(_tree.Device, HWnd, pixelSize.Width, pixelSize.Height, _dpi);

            _wndProc = new PInvoke.WndProc(SafeWndProc);
            PInvoke.SetWindowLongPtr(HWnd, PInvoke.GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_wndProc));

            _items.Updated += RequestRedraw;

            ApplyWindowPos(pixelSize, position);
            PInvoke.ShowWindow(HWnd, PInvoke.SW_SHOWNOACTIVATE);
            PInvoke.InvalidateRect(HWnd, nint.Zero, false);

            if (selectFirstItem) SelectFirstEnabledItem();
        }
        catch
        {
            // The window must never outlive a failed construction, popup and window share one lifetime
            _items.Updated -= RequestRedraw;
            PInvoke.DestroyWindow(HWnd);
            throw;
        }
    }

    /// <summary>
    /// An exception that crosses the native boundary takes down the host process, so every failure
    /// (device loss, a throwing item) closes the tree instead
    /// </summary>
    private nint SafeWndProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        try
        {
            return WndProc(hWnd, msg, wParam, lParam);
        }
        catch (Exception)
        {
            _tree.Dispose();
            return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
        }
    }

    private nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case PInvoke.WM_NCACTIVATE: return 1;
            case WM_APP_POPUP_CALCWND: return HandleCalcWnd();
            case WM_APP_POPUP_KEYDOWN: return HandleKeyDown((int)wParam);
            case PInvoke.WM_TIMER: return HandleTimer(wParam);
            case PInvoke.WM_ERASEBKGND: return 1;
            case PInvoke.WM_PAINT: return HandlePaint(hWnd);

            case PInvoke.WM_MOUSEMOVE: return HandleMouseMove(lParam);
            case PInvoke.WM_MOUSELEAVE: return HandleMouseLeave();
            case PInvoke.WM_LBUTTONDOWN: return HandleMouseButton(lParam, ItemInteractionType.MouseLeftDown);
            case PInvoke.WM_LBUTTONUP: return HandleMouseButton(lParam, ItemInteractionType.MouseLeftUp);
            case PInvoke.WM_RBUTTONDOWN: return HandleMouseButton(lParam, ItemInteractionType.MouseRightDown);
            case PInvoke.WM_RBUTTONUP: return HandleMouseButton(lParam, ItemInteractionType.MouseRightUp);

            case PInvoke.WM_CLOSE: PInvoke.DestroyWindow(hWnd); return 0;
            case PInvoke.WM_DESTROY: return HandleDestroy();
        }

        return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private (Dim PixelSize, Pos Position) ComputeLayout()
    {
        windowLayout = CalculateWindow(_items.GetSnapshot());
        DropStaleHotItem();

        var pixelSize = new Dim(ToPixels(windowLayout.Size.Width), ToPixels(windowLayout.Size.Height));
        return (pixelSize, CalculatePosition(pixelSize));
    }

    private WindowLayout CalculateWindow(ReadOnlySpan<MenuItemBase> items)
    {
        var measuring = new MeasuringContext(_tree.Device, _scale);

        var maxWidth = 0;
        var totalHeight = 0;
        var measuredSizes = new Dim[items.Length];

        for (var i = 0; i < items.Length; i++)
        {
            measuredSizes[i] = items[i].Measure(measuring);
            maxWidth = Math.Max(maxWidth, measuredSizes[i].Width);
            totalHeight += measuredSizes[i].Height;
        }

        var size = new Dim(maxWidth, totalHeight);
        var arranging = new ArrangingContext(_scale, size);

        var arranged = new ItemLayout[items.Length];
        var itemTop = 0;

        for (var i = 0; i < items.Length; i++)
        {
            arranged[i] = ArrangeItem(arranging, items[i], new Rect(0, itemTop, maxWidth, measuredSizes[i].Height), measuredSizes[i]);
            itemTop += measuredSizes[i].Height;
        }

        return new WindowLayout(size, arranged);
    }

    private static ItemLayout ArrangeItem(ArrangingContext context, MenuItemBase item, Rect bounds, Dim measured)
    {
        context.ItemBounds = bounds;
        context.MeasuredItemBounds = bounds with { Width = measured.Width };

        var content = item.Arrange(context);

        var contentWidth = Math.Clamp(content.Width, 0, bounds.Width);
        var contentX = Math.Clamp(content.X, bounds.X, bounds.X + bounds.Width - contentWidth);

        return new ItemLayout
        {
            Item = item,
            HitBounds = bounds,
            ContentBounds = new Rect(contentX, bounds.Y, contentWidth, bounds.Height)
        };
    }

    private Pos CalculatePosition(Dim pixelSize)
    {
        var workArea = PopupPlacement.GetWorkArea(_anchor);

        return _isRoot
            ? PopupPlacement.ResolveRoot(_anchor, pixelSize.Width, pixelSize.Height, workArea)
            : PopupPlacement.ResolveSubmenu(_anchor, pixelSize.Width, pixelSize.Height, workArea, _tree.GetOpenWindowRects(HWnd));
    }

    private void DrawWindow(DrawingContext context)
    {
        context.FillRect(new Rect(0, 0, context.WindowSize.Width, context.WindowSize.Height), _tree.Owner.Background);

        foreach (var entry in windowLayout.Items)
        {
            using (context.Clip(entry.HitBounds))
            {
                context.ItemBounds = entry.ContentBounds;
                entry.Item.Draw(context);
            }
        }
    }

    private ItemLayout? HitTest(Pos point)
    {
        foreach (var entry in windowLayout.Items)
        {
            if (entry.Item.IgnoreInteraction) continue;

            var bounds = entry.HitBounds;
            if (point.X >= bounds.Left && point.X < bounds.Right && point.Y >= bounds.Top && point.Y < bounds.Bottom)
            {
                return entry;
            }
        }

        return null;
    }

    private void DropStaleHotItem()
    {
        if (hotItem is null || IndexOf(windowLayout.Items, hotItem) >= 0) return;

        CloseOpenSubmenu();
        hotItem = null;
    }

    private void ApplyWindowPos(Dim pixelSize, Pos position)
        => PInvoke.SetWindowPos(HWnd, nint.Zero, position.X, position.Y, pixelSize.Width, pixelSize.Height, PInvoke.SWP_NOZORDER | PInvoke.SWP_NOACTIVATE | PInvoke.SWP_NOCOPYBITS);

    private void RequestRedraw() => PInvoke.PostMessage(HWnd, WM_APP_POPUP_CALCWND, nint.Zero, nint.Zero);

    private nint HandleCalcWnd()
    {
        var (pixelSize, position) = ComputeLayout();

        _surface.Resize(pixelSize.Width, pixelSize.Height, _dpi);
        ApplyWindowPos(pixelSize, position);
        PInvoke.InvalidateRect(HWnd, nint.Zero, false);

        return nint.Zero;
    }

    private nint HandlePaint(nint hWnd)
    {
        PInvoke.BeginPaint(hWnd, out var paint);

        try
        {
            var context = new DrawingContext(_surface, _scale, windowLayout.Size);

            using (_surface.BeginDraw())
            {
                DrawWindow(context);
            }
        }
        finally
        {
            PInvoke.EndPaint(hWnd, ref paint);
        }

        return 0;
    }

    private nint HandleMouseMove(nint lParam)
    {
        if (!tracking)
        {
            var tme = new TRACKMOUSEEVENT
            {
                cbSize = (uint)Marshal.SizeOf<TRACKMOUSEEVENT>(),
                dwFlags = PInvoke.TME_LEAVE,
                hwndTrack = HWnd
            };
            PInvoke.TrackMouseEvent(ref tme);
            tracking = true;
        }

        lastPoint = DecodePoint(lParam);

        SetHotItem(HitTest(lastPoint)?.Item, b => RelativePosition(b, lastPoint), ItemInteractionType.MouseEnter, ItemInteractionType.MouseLeave);

        return 0;
    }

    private nint HandleMouseLeave()
    {
        tracking = false;

        if (openSubmenuOwner is not null) return 0;

        SetHotItem(null, b => RelativePosition(b, lastPoint), ItemInteractionType.MouseEnter, ItemInteractionType.MouseLeave);

        return 0;
    }

    private nint HandleMouseButton(nint lParam, ItemInteractionType type)
    {
        var point = DecodePoint(lParam);

        if (HitTest(point) is { } hit)
        {
            Interact(hit.Item, RelativePosition(hit.HitBounds, point), type, selectFirstOnSubmenu: false);
        }

        return 0;
    }

    private nint HandleKeyDown(int vkCode)
    {
        switch (vkCode)
        {
            case PInvoke.VK_DOWN: MoveHot(+1); break;
            case PInvoke.VK_UP: MoveHot(-1); break;
            case PInvoke.VK_RIGHT: OpenHotSubmenu(); break;
            case PInvoke.VK_RETURN: ActivateHot(); break;
            case PInvoke.VK_LEFT: _tree.NavigateBack(); break;
            case PInvoke.VK_ESCAPE: _tree.CloseFromEscape(); break;
        }

        return 0;
    }

    private nint HandleTimer(nint wParam)
    {
        if (wParam != SubmenuTimerId) return 0;

        KillSubmenuTimer();

        if (hotItem?.CanOpenSubmenu(ItemInteractionType.MouseEnter) ?? false)
        {
            OpenSubmenu(hotItem, false);
        }

        return 0;
    }

    private nint HandleDestroy()
    {
        KillSubmenuTimer();
        _items.Updated -= RequestRedraw;

        try
        {
            foreach (var item in _items) item.Cleanup();
        }
        finally
        {
            _surface.Dispose();
            _tree.UnregisterWindow(HWnd);
        }

        return nint.Zero;
    }

    private void SetHotItem(MenuItemBase? newHot, Func<Rect, Pos> positionFor, ItemInteractionType enterType, ItemInteractionType leaveType)
    {
        if (ReferenceEquals(newHot, hotItem)) return;

        KillSubmenuTimer();

        hotItem?.RaiseInteraction(new ItemInteractedEventArgs
        {
            Type = leaveType,
            Position = positionFor(HitBoundsOf(hotItem))
        });

        hotItem = newHot;

        if (!ReferenceEquals(newHot, openSubmenuOwner))
        {
            CloseOpenSubmenu();
        }

        if (newHot is not null)
        {
            var args = new ItemInteractedEventArgs
            {
                Type = enterType,
                Position = positionFor(HitBoundsOf(newHot))
            };
            newHot.RaiseInteraction(args);

            if (newHot.CanOpenSubmenu(enterType))
            {
                StartSubmenuTimer();
            }
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
            _tree.Dispose();
        }
    }

    private void MoveHot(int direction)
    {
        var items = windowLayout.Items;
        if (items.Length == 0) return;

        var currentIndex = IndexOf(items, hotItem);
        var length = items.Length;

        for (var step = 1; step <= length; step++)
        {
            var index = (((currentIndex + direction * step) % length) + length) % length;
            var candidate = items[index].Item;

            if (!candidate.IgnoreInteraction)
            {
                SetHotItem(candidate, CenterOf, ItemInteractionType.KeyboardFocus, ItemInteractionType.KeyboardBlur);
                return;
            }
        }
    }

    private void OpenHotSubmenu()
    {
        if (hotItem is null) return;

        KillSubmenuTimer();
        OpenSubmenu(hotItem, true);
    }

    private void ActivateHot()
    {
        if (hotItem is null) return;

        Interact(hotItem, CenterOf(HitBoundsOf(hotItem)), ItemInteractionType.KeyboardActivate, true);
    }

    private void SelectFirstEnabledItem()
    {
        foreach (var entry in windowLayout.Items)
        {
            if (entry.Item.IgnoreInteraction) continue;

            SetHotItem(entry.Item, CenterOf, ItemInteractionType.KeyboardFocus, ItemInteractionType.KeyboardBlur);
            return;
        }
    }

    private void StartSubmenuTimer()
    {
        PInvoke.SetTimer(HWnd, SubmenuTimerId, SubmenuHoverDelayMs, nint.Zero);
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
        if (ReferenceEquals(openSubmenuOwner, submenu)) return;

        _tree.OpenChild(HWnd, submenu.SubmenuItems, ToScreenRect(HitBoundsOf(submenu)), selectFirstItem);
        openSubmenuOwner = submenu;
    }

    private void CloseOpenSubmenu()
    {
        if (openSubmenuOwner is null) return;

        _tree.CloseChildrenOf(HWnd);
        openSubmenuOwner = null;
    }

    private Rect HitBoundsOf(MenuItemBase item)
    {
        foreach (var entry in windowLayout.Items)
        {
            if (ReferenceEquals(entry.Item, item)) return entry.HitBounds;
        }

        return default;
    }

    private Rect ToScreenRect(Rect localDips)
    {
        var topLeft = new POINT
        {
            X = ToPixelsRounded(localDips.Left),
            Y = ToPixelsRounded(localDips.Top)
        };
        var bottomRight = new POINT
        {
            X = ToPixelsRounded(localDips.Right),
            Y = ToPixelsRounded(localDips.Bottom)
        };

        PInvoke.ClientToScreen(HWnd, ref topLeft);
        PInvoke.ClientToScreen(HWnd, ref bottomRight);

        return new Rect(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
    }

    private int ToPixels(int dips) => (int)MathF.Ceiling(dips * _scale - 0.001f);

    private int ToPixelsRounded(int dips) => (int)MathF.Round(dips * _scale);

    private Pos DecodePoint(nint lParam)
    {
        var value = (int)lParam.ToInt64();
        var x = unchecked((short)(value & 0xFFFF));
        var y = unchecked((short)((value >> 16) & 0xFFFF));

        return new Pos((int)MathF.Floor(x / _scale), (int)MathF.Floor(y / _scale));
    }

    private static int IndexOf(ReadOnlySpan<ItemLayout> items, MenuItemBase? item)
    {
        if (item is null) return -1;

        for (var i = 0; i < items.Length; i++)
        {
            if (ReferenceEquals(items[i].Item, item)) return i;
        }

        return -1;
    }

    private static Pos RelativePosition(Rect bounds, Pos point) => new(point.X - bounds.X, point.Y - bounds.Y);

    private static Pos CenterOf(Rect bounds) => new(bounds.Width / 2, bounds.Height / 2);
}