namespace DotTray.Internal;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using DotTray.Popup.Default;
using DotTray.Popup.Default.Context;
using DotTray.Primitives;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

internal sealed class PopupMenu
{
    private const float BaseDpi = 96f;
    private const uint SubmenuHoverDelayMs = 350;
    private const nint SubmenuTimerId = 1;

    public const uint WM_APP_POPUP_CALCWND = PInvoke.WM_APP + 0x2000;
    public const uint WM_APP_POPUP_KEYDOWN = PInvoke.WM_APP + 0x2001;

    private readonly float _scale;
    private readonly PInvoke.WndProc _wndProc;
    private readonly PopupMenuTree _tree;

    private readonly Rectangle _rootCursorAnchor;
    private readonly MenuItemCollection _items;
    private readonly Rectangle? _anchorScreenRect;

    private MenuItemBase? hotItem;
    private MenuItemBase? openSubmenuOwner;
    private bool submenuTimerActive;
    private bool tracking;
    private POINT lastPoint;

    public nint HWnd { get; }

    public PopupMenu(PopupMenuTree tree, nint ownerHWnd, MenuItemCollection items, Rectangle? anchorScreenRect, bool selectFirstItem = false)
    {
        _tree = tree;
        _items = items;
        _anchorScreenRect = anchorScreenRect;
        _rootCursorAnchor = anchorScreenRect ?? GetCursorAnchor();

        _items.Updated += RequestRedraw;

        foreach (var item in _items) item.Initialize();

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

        if (selectFirstItem) SelectFirstEnabledItem();
    }

    private void RequestRedraw() => PInvoke.PostMessage(HWnd, WM_APP_POPUP_CALCWND, nint.Zero, nint.Zero);

    private nint WndProcFunc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case PInvoke.WM_NCACTIVATE: return 1;
            case WM_APP_POPUP_CALCWND: return HandleCalcWnd(hWnd);
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

    private nint HandleCalcWnd(nint hWnd)
    {
        var rect = CalcWindowArea(_items);
        PInvoke.SetWindowPos(hWnd, nint.Zero, rect.X, rect.Y, rect.Width, rect.Height, PInvoke.SWP_ZORDER | PInvoke.SWP_NOACTIVATE);
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
            PInvoke.GdipScaleWorldTransform(gdip, _scale, _scale, PInvoke.MatrixOrderPrepend);

            using (var hBackground = _tree.Owner.Handler.Color.CreateGdipBrush(bounds))
            {
                PInvoke.GdipFillRectangleI(gdip, hBackground.DangerousGetHandle(), bounds.X, bounds.Y, bounds.Width, bounds.Height);
            }

            using (var drawing = new DrawingContext(gdip, _scale, bounds))
            {
                foreach (var item in _items)
                {
                    var hit = UnscaleRect(item.HitBounds, _scale);
                    PInvoke.GdipSetClipRectI(gdip, hit.X, hit.Y, hit.Width, hit.Height, PInvoke.CombineModeReplace);

                    drawing.ItemBounds = UnscaleRect(item.ContentBounds, _scale);
                    item.Draw(drawing);
                }
            }

            PInvoke.GdipResetClip(gdip);
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

        lastPoint = DecodePoint(lParam);

        SetHotItem(HitTest(lastPoint), b => RelativePosition(b, lastPoint), ItemInteractionType.MouseEnter, ItemInteractionType.MouseLeave);

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
        var hit = HitTest(point);

        if (hit is not null) Interact(hit, RelativePosition(hit.HitBounds, point), type, selectFirstOnSubmenu: false);

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

    private void SetHotItem(MenuItemBase? newHot, Func<Rectangle, Point> positionFor, ItemInteractionType enterType, ItemInteractionType leaveType)
    {
        if (ReferenceEquals(newHot, hotItem)) return;

        KillSubmenuTimer();

        hotItem?.RaiseInteraction(new ItemInteractedEventArgs
        {
            Type = leaveType,
            Position = positionFor(hotItem.HitBounds)
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
                Position = positionFor(newHot.HitBounds)
            };
            newHot.RaiseInteraction(args);

            if (newHot.CanOpenSubmenu(enterType))
            {
                StartSubmenuTimer();
            }
        }

        PInvoke.InvalidateRect(HWnd, nint.Zero, false);
    }

    private void Interact(MenuItemBase item, Point position, ItemInteractionType type, bool selectFirstOnSubmenu)
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
        if (_items.IsEmpty) return;

        var currentIndex = hotItem is null ? -1 : _items.IndexOf(hotItem);
        var count = _items.Count;

        for (var step = 1; step <= count; step++)
        {
            var index = (((currentIndex + direction * step) % count) + count) % count;
            var candidate = _items[index];

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

        Interact(hotItem, CenterOf(hotItem.HitBounds), ItemInteractionType.KeyboardActivate, true);
    }

    private void SelectFirstEnabledItem()
    {
        foreach (var item in _items)
        {
            if (item.IgnoreInteraction) continue;

            SetHotItem(item, CenterOf, ItemInteractionType.KeyboardFocus, ItemInteractionType.KeyboardBlur);
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

        var screenRect = ToScreenRect(submenu.HitBounds);

        _tree.OpenChild(HWnd, submenu.SubmenuItems, screenRect, selectFirstItem);
        openSubmenuOwner = submenu;
    }

    private void CloseOpenSubmenu()
    {
        if (openSubmenuOwner is null) return;

        _tree.CloseChildrenOf(HWnd);
        openSubmenuOwner = null;
    }

    private MenuItemBase? HitTest(POINT point)
    {
        foreach (var item in _items)
        {
            if (item.IgnoreInteraction) continue;

            var bounds = item.HitBounds;
            if (point.x >= bounds.Left && point.x < bounds.Right && point.y >= bounds.Top && point.y < bounds.Bottom)
            {
                return item;
            }
        }

        return null;
    }

    private Rectangle ToScreenRect(Rectangle localRect)
    {
        var topLeft = new POINT
        {
            x = localRect.Left,
            y = localRect.Top
        };
        var bottomRight = new POINT
        {
            x = localRect.Right,
            y = localRect.Bottom
        };

        PInvoke.ClientToScreen(HWnd, ref topLeft);
        PInvoke.ClientToScreen(HWnd, ref bottomRight);

        return new Rectangle(topLeft.x, topLeft.y, bottomRight.x - topLeft.x, bottomRight.y - topLeft.y);
    }

    private nint HandleDestroy()
    {
        KillSubmenuTimer();
        _items.Updated -= RequestRedraw;

        foreach (var item in _items) item.Cleanup();

        _tree.UnregisterWindow(HWnd);
        return nint.Zero;
    }

    private Rectangle CalcWindowArea(MenuItemCollection items)
    {
        var hdc = PInvoke.CreateCompatibleDC(nint.Zero);
        _ = PInvoke.GdipCreateFromHDC(hdc, out var gdip);

        PInvoke.GdipScaleWorldTransform(gdip, _scale, _scale, PInvoke.MatrixOrderPrepend);

        var maxWidthLogical = 0;
        var totalHeightLogical = 0;

        var measuredSizes = new Size[items.Count];

        using (var measuring = new MeasuringContext(gdip, _scale))
        {
            for (var i = 0; i < items.Count; i++)
            {
                measuredSizes[i] = items[i].Measure(measuring);
                maxWidthLogical = Math.Max(maxWidthLogical, measuredSizes[i].Width);
                totalHeightLogical += measuredSizes[i].Height;
            }
        }

        using (var arranging = new ArrangingContext(gdip, _scale, new Size(maxWidthLogical, totalHeightLogical)))
        {
            var itemTop = 0;

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var desired = measuredSizes[i];
                var fullRect = new Rectangle(0, itemTop, maxWidthLogical, desired.Height);

                arranging.ItemBounds = fullRect;
                arranging.MeasuredItemBounds = new Rectangle(0, itemTop, desired.Width, desired.Height);

                var content = item.Arrange(arranging);

                var contentWidth = Math.Clamp(content.Width, 0, maxWidthLogical);
                var contentX = Math.Clamp(content.X, 0, maxWidthLogical - contentWidth);
                var contentRect = new Rectangle(contentX, itemTop, contentWidth, desired.Height);

                item.HitBounds = ScaleRect(fullRect, _scale);
                item.ContentBounds = ScaleRect(contentRect, _scale);

                itemTop += desired.Height;
            }
        }

        _ = PInvoke.GdipDeleteGraphics(gdip);
        _ = PInvoke.DeleteDC(hdc);

        var maxWidth = (int)MathF.Round(maxWidthLogical * _scale);
        var totalHeight = (int)MathF.Round(totalHeightLogical * _scale);

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

    private static Point RelativePosition(Rectangle bounds, POINT point) => new(point.x - bounds.X, point.y - bounds.Y);

    private static Point CenterOf(Rectangle bounds) => new(bounds.Width / 2, bounds.Height / 2);

    private static Rectangle ScaleRect(Rectangle rect, float scale) => new(
        (int)MathF.Ceiling(rect.X * scale),
        (int)MathF.Ceiling(rect.Y * scale),
        (int)MathF.Ceiling(rect.Width * scale),
        (int)MathF.Ceiling(rect.Height * scale));

    private static Rectangle UnscaleRect(Rectangle rect, float scale) => new(
        (int)MathF.Ceiling(rect.X / scale),
        (int)MathF.Ceiling(rect.Y / scale),
        (int)MathF.Ceiling(rect.Width / scale),
        (int)MathF.Ceiling(rect.Height / scale));
}