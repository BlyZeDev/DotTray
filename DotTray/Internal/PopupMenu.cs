namespace DotTray.Internal;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using DotTray.Popup.Default;
using DotTray.Popup.Default.Context;
using DotTray.Primitives;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using HitItem = (Popup.Default.MenuItemBase Item, Primitives.Rectangle Bounds);
using MeasuredItem = (Popup.Default.MenuItemBase Item, Primitives.Size Size);

internal sealed class PopupMenu
{
    private const float BaseDpi = 96f;
    public const uint WM_APP_POPUP_CALCWND = PInvoke.WM_APP + 0x2000;

    private readonly float _scale;
    private readonly PInvoke.WndProc _wndProc;
    private readonly PopupMenuTree _tree;

    private readonly Rectangle _rootCursorAnchor;
    private readonly MenuItemCollection _items;
    private readonly Rectangle? _anchorScreenRect;

    private readonly List<HitItem> _itemRects = [];
    private readonly List<MeasuredItem> _measuredSizes = [];

    private MenuItemBase? hotItem;
    private ISubmenu? openSubmenuOwner;
    private bool tracking;
    private POINT lastPoint;

    public nint HWnd { get; }

    public PopupMenu(PopupMenuTree tree, nint ownerHWnd, MenuItemCollection items, Rectangle? anchorScreenRect)
    {
        _tree = tree;
        _items = items;
        _anchorScreenRect = anchorScreenRect;
        _rootCursorAnchor = anchorScreenRect ?? GetCursorAnchor();

        _items.Updated += RequestRedraw;

        HWnd = PInvoke.CreateWindowEx(
            PInvoke.WS_EX_NOACTIVATE | PInvoke.WS_EX_TOOLWINDOW | PInvoke.WS_EX_TOPMOST,
            _tree.Owner.PopupWindowClassName, nint.Zero,
            PInvoke.WS_CLIPCHILDREN | PInvoke.WS_CLIPSIBLINGS | PInvoke.WS_POPUP,
            0, 0, 0, 0,
            ownerHWnd,
            nint.Zero,
            _tree.Owner.InstanceHandle,
            nint.Zero);

        _scale = PInvoke.GetDpiForWindow(HWnd) / BaseDpi;

        var cornerRadius = PInvoke.DWMWCP_ROUND;
        PInvoke.DwmSetWindowAttribute(HWnd, PInvoke.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerRadius, sizeof(int));

        var borderColor = PInvoke.DWMWA_COLOR_NONE;
        PInvoke.DwmSetWindowAttribute(HWnd, PInvoke.DWMWA_BORDER_COLOR, ref borderColor, sizeof(int));

        _wndProc = new PInvoke.WndProc(WndProcFunc);
        PInvoke.SetWindowLongPtr(HWnd, PInvoke.GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_wndProc));

        HandleCalcWnd(HWnd);
        PInvoke.ShowWindow(HWnd, PInvoke.SW_SHOWNOACTIVATE);
    }

    private void RequestRedraw() => PInvoke.PostMessage(HWnd, WM_APP_POPUP_CALCWND, nint.Zero, nint.Zero);

    private nint WndProcFunc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case PInvoke.WM_NCACTIVATE: return 1;
            case WM_APP_POPUP_CALCWND: return HandleCalcWnd(hWnd);
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

    private nint HandleCalcWnd(nint hWnd)
    {
        var rect = CalcWindowArea(_items);
        PInvoke.SetWindowPos(hWnd, nint.Zero, rect.X, rect.Y, rect.Width, rect.Height, PInvoke.SWP_ZORDER | PInvoke.SWP_NOACTIVATE);

        if (hotItem is ISubmenu submenu)
        {
            var wouldOpen = submenu.ShouldOpen(new ItemInteractedEventArgs
            {
                Type = ItemInteractionType.MouseEnter,
                Position = default
            });

            if (wouldOpen && !ReferenceEquals(openSubmenuOwner, submenu))
            {
                OpenSubmenu(submenu);
            }
            else if (!wouldOpen && ReferenceEquals(openSubmenuOwner, submenu))
            {
                CloseOpenSubmenu();
            }
        }

        PInvoke.InvalidateRect(hWnd, nint.Zero, false);

        return nint.Zero;
    }

    private nint HandlePaint(nint hWnd)
    {
        var hPaint = PInvoke.BeginPaint(hWnd, out var paint);

        try
        {
            PInvoke.GetClientRect(hWnd, out var cRect);
            var bounds = new Rectangle
            {
                X = cRect.Left,
                Y = cRect.Top,
                Width = cRect.Right - cRect.Left,
                Height = cRect.Bottom - cRect.Top
            };

            var dc = PInvoke.CreateCompatibleDC(hPaint);
            var hBitmap = PInvoke.CreateCompatibleBitmap(hPaint, bounds.Width, bounds.Height);
            var hOldBitmap = PInvoke.SelectObject(dc, hBitmap);

            PInvoke.GdipCreateFromHDC(dc, out var gdip);

            using (var hBackground = _tree.Owner.Handler.Color.CreateGdipBrush(bounds))
            {
                PInvoke.GdipFillRectangleI(gdip, hBackground.DangerousGetHandle(), bounds.X, bounds.Y, bounds.Width, bounds.Height);
            }

            using (var drawing = new DrawingContext(gdip, _scale, bounds))
            {
                foreach (var (item, itemBounds) in _itemRects)
                {
                    drawing.ItemBounds = itemBounds;
                    item.Draw(drawing);
                }
            }

            PInvoke.GdipDeleteGraphics(gdip);

            PInvoke.BitBlt(hPaint, 0, 0, bounds.Width, bounds.Height, dc, 0, 0, PInvoke.SRCCOPY);

            PInvoke.SelectObject(dc, hOldBitmap);
            PInvoke.DeleteObject(hBitmap);
            PInvoke.DeleteDC(dc);
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

        var point = DecodePoint(lParam);
        var hit = HitTest(point);

        UpdateHotItem(hit, lastPoint, point);

        lastPoint = point;

        return 0;
    }

    private nint HandleMouseLeave()
    {
        tracking = false;

        if (hotItem is not null)
        {
            var bounds = FindBounds(hotItem) ?? default;
            hotItem.OnInteraction(new ItemInteractedEventArgs
            {
                Type = ItemInteractionType.MouseLeave,
                Position = RelativePosition(bounds, lastPoint)
            });
        }

        hotItem = null;

        return 0;
    }

    private nint HandleMouseButton(nint lParam, ItemInteractionType type)
    {
        var point = DecodePoint(lParam);
        var hit = HitTest(point);

        if (!hit.HasValue) return 0;

        var args = new ItemInteractedEventArgs
        {
            Type = type,
            Position = RelativePosition(hit.Value.Bounds, point)
        };
        hit.Value.Item.OnInteraction(args);

        if (hit.Value.Item is ISubmenu submenu && submenu.ShouldOpen(args))
        {
            OpenSubmenu(submenu);
        }

        return 0;
    }

    private void UpdateHotItem(HitItem? hit, POINT previousPoint, POINT currentPoint)
    {
        var newHot = hit?.Item;
        if (ReferenceEquals(newHot, hotItem)) return;

        if (hotItem is not null)
        {
            var oldBounds = FindBounds(hotItem) ?? default;
            hotItem.OnInteraction(new ItemInteractedEventArgs
            {
                Type = ItemInteractionType.MouseLeave,
                Position = RelativePosition(oldBounds, previousPoint)
            });
        }

        hotItem = newHot;

        if (!ReferenceEquals(newHot, openSubmenuOwner))
        {
            CloseOpenSubmenu();
        }

        if (hit.HasValue)
        {
            var args = new ItemInteractedEventArgs
            {
                Type = ItemInteractionType.MouseEnter,
                Position = RelativePosition(hit.Value.Bounds, currentPoint)
            };
            hit.Value.Item.OnInteraction(args);

            if (hit.Value.Item is ISubmenu submenu && submenu.ShouldOpen(args))
            {
                OpenSubmenu(submenu);
            }
        }
    }

    private void OpenSubmenu(ISubmenu submenu)
    {
        if (ReferenceEquals(openSubmenuOwner, submenu)) return;

        var localBounds = FindBounds((MenuItemBase)submenu);
        if (localBounds is null) return;

        var screenRect = ToScreenRect(localBounds.Value);

        _tree.OpenChild(HWnd, submenu.Items, screenRect);
        openSubmenuOwner = submenu;
    }

    private void CloseOpenSubmenu()
    {
        if (openSubmenuOwner is null) return;

        _tree.CloseChildrenOf(HWnd);
        openSubmenuOwner = null;
    }

    private HitItem? HitTest(POINT point)
    {
        foreach (var entry in _itemRects)
        {
            if (entry.Item.IsDisabled) continue;

            var bounds = entry.Bounds;
            if (point.x >= bounds.Left && point.x < bounds.Right && point.y >= bounds.Top && point.y < bounds.Bottom)
            {
                return entry;
            }
        }

        return null;
    }

    private Rectangle? FindBounds(MenuItemBase item)
    {
        foreach (var (candidate, bounds) in _itemRects)
        {
            if (ReferenceEquals(candidate, item)) return bounds;
        }

        return null;
    }

    private Rectangle ToScreenRect(Rectangle localRect)
    {
        var topLeft = new POINT { x = localRect.Left, y = localRect.Top };
        var bottomRight = new POINT { x = localRect.Right, y = localRect.Bottom };

        PInvoke.ClientToScreen(HWnd, ref topLeft);
        PInvoke.ClientToScreen(HWnd, ref bottomRight);

        return new Rectangle(topLeft.x, topLeft.y, bottomRight.x - topLeft.x, bottomRight.y - topLeft.y);
    }

    private nint HandleDestroy()
    {
        _items.Updated -= RequestRedraw;
        _tree.UnregisterWindow(HWnd);
        return nint.Zero;
    }

    private Rectangle CalcWindowArea(MenuItemCollection items)
    {
        var hdc = PInvoke.CreateCompatibleDC(nint.Zero);
        _ = PInvoke.GdipCreateFromHDC(hdc, out var gdip);

        var maxWidth = 0;
        var totalHeight = 0;

        _itemRects.Clear();
        _measuredSizes.Clear();

        using (var measuring = new MeasuringContext(gdip, _scale))
        {
            foreach (var item in items)
            {
                var desired = item.Measure(measuring);
                _measuredSizes.Add((item, desired));

                maxWidth = Math.Max(maxWidth, desired.Width);
                totalHeight += desired.Height;
            }
        }

        using (var arranging = new ArrangingContext(gdip, _scale, new Size(maxWidth, totalHeight)))
        {
            var itemTop = 0;

            foreach (var (item, desired) in _measuredSizes)
            {
                arranging.ItemBounds = new Rectangle(0, itemTop, maxWidth, desired.Height);
                arranging.MeasuredItemBounds = new Rectangle(0, itemTop, desired.Width, desired.Height);

                var requested = item.Arrange(arranging);

                var width = Math.Clamp(requested.Width, 0, maxWidth);
                var x = Math.Clamp(requested.X, 0, maxWidth - width);

                item.DrawBox = new Size(width, desired.Height);
                _itemRects.Add((item, new Rectangle(x, itemTop, width, desired.Height)));
                itemTop += desired.Height;
            }
        }

        _ = PInvoke.GdipDeleteGraphics(gdip);
        _ = PInvoke.DeleteDC(hdc);

        var anchor = _anchorScreenRect ?? _rootCursorAnchor;
        var hMonitor = PInvoke.MonitorFromPoint(new POINT { x = anchor.X, y = anchor.Y }, PInvoke.MONITOR_DEFAULTTONEAREST);
        var workArea = GetMonitorWorkArea(hMonitor);

        var pos = _anchorScreenRect.HasValue
            ? ResolveSubmenuPosition(anchor, maxWidth, totalHeight, workArea, _tree.GetOpenWindowRects(HWnd))
            : ResolveRootPosition(anchor, maxWidth, totalHeight, workArea);

        return new Rectangle(pos.X, pos.Y, maxWidth, totalHeight);
    }

    private static POINT DecodePoint(nint lParam)
    {
        var value = (int)lParam.ToInt64();
        return new POINT
        {
            x = unchecked((short)(value & 0xFFFF)),
            y = unchecked((short)((value >> 16) & 0xFFFF))
        };
    }

    private static Rectangle GetCursorAnchor()
    {
        PInvoke.GetCursorPos(out var pos);
        return new Rectangle(pos.x, pos.y, 0, 0);
    }

    private static Point ResolveRootPosition(Rectangle anchor, int width, int height, Rectangle workArea)
    {
        var x = anchor.X;
        var y = anchor.Y;

        if (x + width > workArea.Right) x = anchor.X - width;
        if (y + height > workArea.Bottom) y = anchor.Y - height;

        return ClampToWorkArea(x, y, width, height, workArea);
    }

    private static Point ResolveSubmenuPosition(Rectangle anchor, int width, int height, Rectangle workArea, List<Rectangle> obstacles)
    {
        var y = anchor.Top;
        if (y + height > workArea.Bottom) y = anchor.Bottom - height;
        if (y < workArea.Top) y = workArea.Top;

        var rightX = anchor.Right;
        var leftX = anchor.Left - width;

        var rightFits = rightX + width <= workArea.Right;
        var leftFits = leftX >= workArea.Left;

        var rightRect = new Rectangle(rightX, y, width, height);
        var leftRect = new Rectangle(leftX, y, width, height);

        var rightOverlaps = rightFits ? CountOverlaps(rightRect, obstacles) : -1;
        var leftOverlaps = leftFits ? CountOverlaps(leftRect, obstacles) : -1;

        int x;
        if (rightFits && rightOverlaps == 0) x = rightX;
        else if (leftFits && leftOverlaps == 0) x = leftX;
        else if (rightFits && leftFits) x = rightOverlaps <= leftOverlaps ? rightX : leftX;
        else if (rightFits) x = rightX;
        else if (leftFits) x = leftX;
        else x = rightX;

        return ClampToWorkArea(x, y, width, height, workArea);
    }

    private static int CountOverlaps(Rectangle rect, IReadOnlyCollection<Rectangle> obstacles)
    {
        var count = 0;
        foreach (var obstacle in obstacles)
        {
            if (rect.Left < obstacle.Right && rect.Right > obstacle.Left && rect.Top < obstacle.Bottom && rect.Bottom > obstacle.Top) count++;
        }

        return count;
    }

    private static Rectangle GetMonitorWorkArea(nint monitorHandle)
    {
        var monitorInfo = new MONITORINFO
        {
            cbSize = (uint)Marshal.SizeOf<MONITORINFO>()
        };
        PInvoke.GetMonitorInfo(monitorHandle, ref monitorInfo);

        return new Rectangle(
            monitorInfo.rcWork.Left,
            monitorInfo.rcWork.Top,
            monitorInfo.rcWork.Right - monitorInfo.rcWork.Left,
            monitorInfo.rcWork.Bottom - monitorInfo.rcWork.Top);
    }

    private static Point ClampToWorkArea(int x, int y, int width, int height, Rectangle workArea)
    {
        x = Math.Clamp(x, workArea.Left, Math.Max(workArea.Left, workArea.Right - width));
        y = Math.Clamp(y, workArea.Top, Math.Max(workArea.Top, workArea.Bottom - height));

        return new Point(x, y);
    }

    private static Point RelativePosition(Rectangle bounds, POINT point) => new Point(point.x - bounds.X, point.y - bounds.Y);
}