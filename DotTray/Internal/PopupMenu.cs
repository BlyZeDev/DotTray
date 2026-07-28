namespace DotTray.Internal;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using DotTray.Popup.Default;
using DotTray.Primitives;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Hit = (Popup.Default.MenuItemBase Item, Primitives.Rectangle Bounds);
using WindowArea = (int X, int Y, int Width, int Height);
using ScreenPosition = (int X, int Y);
using MonitorSize = (int Width, int Height);

internal sealed class PopupMenu
{
    private const float BaseDpi = 96f;
    public const uint WM_APP_POPUP_CALCWND = PInvoke.WM_APP + 0x2000;

    private readonly float _scale;
    private readonly PInvoke.WndProc _wndProc;
    private readonly PopupMenuTree _tree;

    private readonly MenuItemCollection _items;
    private readonly Rectangle? _anchorScreenRect;

    private readonly List<Hit> _itemRects = [];

    private MenuItemBase? _hotItem;
    private SubmenuItemBase? _openSubmenuOwner;
    private bool _tracking;
    private POINT _lastPoint;

    public nint HWnd { get; }

    public PopupMenu(PopupMenuTree tree, nint ownerHWnd, MenuItemCollection items, Rectangle? anchorScreenRect)
    {
        _tree = tree;
        _items = items;
        _anchorScreenRect = anchorScreenRect;

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
            case PInvoke.WM_SIZE: PInvoke.InvalidateRect(hWnd, nint.Zero, false); return 0;
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
        var (x, y, width, height) = CalcWindowArea(_items);
        PInvoke.SetWindowPos(hWnd, nint.Zero, x, y, width, height, PInvoke.SWP_ZORDER | PInvoke.SWP_NOACTIVATE);

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

            using (var hBackground = _tree.Owner.Handler.Color.CreateNativeHandle(bounds))
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
        if (!_tracking)
        {
            var tme = new TRACKMOUSEEVENT
            {
                cbSize = (uint)Marshal.SizeOf<TRACKMOUSEEVENT>(),
                dwFlags = PInvoke.TME_LEAVE,
                hwndTrack = HWnd
            };
            PInvoke.TrackMouseEvent(ref tme);
            _tracking = true;
        }

        var point = DecodePoint(lParam);
        var hit = HitTest(point);

        UpdateHotItem(hit, _lastPoint, point);

        _lastPoint = point;

        return 0;
    }

    private nint HandleMouseLeave()
    {
        _tracking = false;

        if (_hotItem is not null)
        {
            var bounds = FindBounds(_hotItem) ?? default;
            _hotItem.OnInteraction(new ItemInteractedEventArgs
            {
                Type = ItemInteractionType.MouseLeave,
                Position = RelativePosition(bounds, _lastPoint)
            });
        }

        _hotItem = null;

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

        if (hit.Value.Item is SubmenuItemBase submenu && submenu.ShouldOpen(args))
        {
            OpenSubmenu(submenu);
        }

        return 0;
    }

    private void UpdateHotItem(Hit? hit, POINT previousPoint, POINT currentPoint)
    {
        var newHot = hit?.Item;
        if (ReferenceEquals(newHot, _hotItem)) return;

        if (_hotItem is not null)
        {
            var oldBounds = FindBounds(_hotItem) ?? default;
            _hotItem.OnInteraction(new ItemInteractedEventArgs
            {
                Type = ItemInteractionType.MouseLeave,
                Position = RelativePosition(oldBounds, previousPoint)
            });
        }

        _hotItem = newHot;

        if (!ReferenceEquals(newHot, _openSubmenuOwner))
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

            if (hit.Value.Item is SubmenuItemBase submenu && submenu.ShouldOpen(args))
            {
                OpenSubmenu(submenu);
            }
        }
    }

    private void OpenSubmenu(SubmenuItemBase submenu)
    {
        if (submenu.Items.IsEmpty) return;
        if (ReferenceEquals(_openSubmenuOwner, submenu)) return;

        var localBounds = FindBounds(submenu);
        if (localBounds is null) return;

        var screenRect = ToScreenRect(localBounds.Value);

        _tree.OpenChild(HWnd, submenu.Items, screenRect);
        _openSubmenuOwner = submenu;
    }

    private void CloseOpenSubmenu()
    {
        if (_openSubmenuOwner is null) return;

        _tree.CloseChildrenOf(HWnd);
        _openSubmenuOwner = null;
    }

    private Hit? HitTest(POINT point)
    {
        foreach (var entry in _itemRects)
        {
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

    private WindowArea CalcWindowArea(MenuItemCollection items)
    {
        var hdc = PInvoke.CreateCompatibleDC(nint.Zero);
        _ = PInvoke.GdipCreateFromHDC(hdc, out var gdip);

        var maxWidth = 0;
        var totalHeight = 0;

        _itemRects.Clear();

        using (var measuring = new MeasuringContext(gdip, _scale))
        {
            var itemTop = 0;

            foreach (var item in items)
            {
                item.DrawBox = item.Measure(measuring);
                _itemRects.Add((item, new Rectangle(0, itemTop, item.DrawBox.Width, item.DrawBox.Height)));

                maxWidth = Math.Max(maxWidth, item.DrawBox.Width);
                itemTop += item.DrawBox.Height;
                totalHeight = itemTop;
            }
        }

        _ = PInvoke.GdipDeleteGraphics(gdip);
        _ = PInvoke.DeleteDC(hdc);

        var anchor = _anchorScreenRect ?? GetCursorAnchor();
        var hMonitor = PInvoke.MonitorFromPoint(new POINT { x = anchor.X, y = anchor.Y }, PInvoke.MONITOR_DEFAULTTONEAREST);
        var (screenWidth, screenHeight) = GetMonitorWorkArea(hMonitor);

        var (x, y) = _anchorScreenRect.HasValue
            ? ResolveSubmenuPosition(anchor, maxWidth, totalHeight, screenWidth, screenHeight, _tree.GetOpenWindowRects(HWnd))
            : ResolveRootPosition(anchor, maxWidth, totalHeight, screenWidth, screenHeight);

        return (x, y, maxWidth, totalHeight);
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

    private static ScreenPosition ResolveRootPosition(Rectangle anchor, int width, int height, int screenWidth, int screenHeight)
    {
        var x = anchor.X;
        var y = anchor.Y;

        if (x + width > screenWidth) x = Math.Abs(x - width);
        if (y + height > screenHeight) y = Math.Abs(y - height);

        return (x, y);
    }

    private static ScreenPosition ResolveSubmenuPosition(Rectangle anchor, int width, int height, int screenWidth, int screenHeight, IReadOnlyCollection<Rectangle> obstacles)
    {
        var y = anchor.Top;
        if (y + height > screenHeight) y = anchor.Bottom - height;
        if (y < 0) y = 0;

        var rightX = anchor.Right;
        var leftX = anchor.Left - width;

        var rightFits = rightX + width <= screenWidth;
        var leftFits = leftX >= 0;

        var rightRect = new Rectangle(rightX, y, width, height);
        var leftRect = new Rectangle(leftX, y, width, height);

        var rightOverlaps = rightFits ? CountOverlaps(rightRect, obstacles) : -1;
        var leftOverlaps = leftFits ? CountOverlaps(leftRect, obstacles) : -1;

        if (rightFits && rightOverlaps == 0) return (rightX, y);
        if (leftFits && leftOverlaps == 0) return (leftX, y);

        if (rightFits && leftFits) return rightOverlaps <= leftOverlaps ? (rightX, y) : (leftX, y);
        if (rightFits) return (rightX, y);
        if (leftFits) return (leftX, y);

        return (Math.Clamp(rightX, 0, Math.Max(0, screenWidth - width)), y);
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

    private static MonitorSize GetMonitorWorkArea(nint monitorHandle)
    {
        var monitorInfo = new MONITORINFO
        {
            cbSize = (uint)Marshal.SizeOf<MONITORINFO>()
        };
        PInvoke.GetMonitorInfo(monitorHandle, ref monitorInfo);

        return (monitorInfo.rcWork.Right - monitorInfo.rcWork.Left, monitorInfo.rcWork.Bottom - monitorInfo.rcWork.Top);
    }

    private static Point RelativePosition(Rectangle bounds, POINT point) => new Point(point.x - bounds.X, point.y - bounds.Y);
}