namespace DotTray.Internal;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;

internal sealed partial class PopupMenu
{
    private unsafe nint HandlePaint(nint hWnd)
    {
        var paintHandle = PInvoke.BeginPaint(hWnd, out var paint);

        try
        {
            PInvoke.GetClientRect(hWnd, out var clientRect);

            var memoryDC = PInvoke.CreateCompatibleDC(paintHandle);
            var bitmapHandle = PInvoke.CreateCompatibleBitmap(paintHandle, clientRect.Right - clientRect.Left, clientRect.Bottom - clientRect.Top);
            var oldBitmapHandle = PInvoke.SelectObject(memoryDC, bitmapHandle);

            _ = PInvoke.GdipCreateFromHDC(memoryDC, out var graphicsHandle);
            _ = PInvoke.GdipSetSmoothingMode(graphicsHandle, PInvoke.SmoothingModeAntiAlias8x8);

            DrawMenuBackground(graphicsHandle, clientRect, _session.OwnerIcon.PopupMenuColor.ToGdiPlus());

            _ = PInvoke.GdipCreateFontFamilyFromName(FontFamilyName, nint.Zero, out var fontFamily);
            _ = PInvoke.GdipCreateFont(fontFamily, _layout.FontSizePx, 0, PInvoke.UnitPixel, out var font);
            _ = PInvoke.GdipGetFontHeight(font, graphicsHandle, out var fontHeight);

            POINTF* checkBoxPoints = stackalloc POINTF[CheckBoxPoints];
            POINTF* submenuArrowPoints = stackalloc POINTF[ArrowPoints];
            var itemTop = (float)clientRect.Top;
            for (int i = 0; i < _menuItems.Count; i++)
            {
                var itemRect = new RECTF
                {
                    X = clientRect.Left,
                    Y = itemTop,
                    Width = clientRect.Right - clientRect.Left,
                    Height = _menuItems[i].HeightMultiplier * _layout.FontSizePx
                };

                if (_menuItems[i] is MenuItem menuItem)
                {
                    menuItem.HitBox = itemRect;

                    var backgroundColor = (menuItem.IsDisabled ? menuItem.BackgroundDisabledColor : (i == hoverIndex ? menuItem.BackgroundHoverColor : menuItem.BackgroundColor)).ToGdiPlus();
                    var textColor = (menuItem.IsDisabled ? menuItem.TextDisabledColor : (i == hoverIndex ? menuItem.TextHoverColor : menuItem.TextColor)).ToGdiPlus();

                    DrawMenuItem(graphicsHandle, _layout, menuItem, font, backgroundColor, textColor, checkBoxPoints, submenuArrowPoints);
                }
                else if (_menuItems[i] is SeparatorItem separatorItem) DrawSeparatorItem(graphicsHandle, _layout, separatorItem, itemRect);

                itemTop += itemRect.Height;
            }

            _ = PInvoke.GdipDeleteFont(font);
            _ = PInvoke.GdipDeleteFontFamily(fontFamily);
            _ = PInvoke.GdipDeleteGraphics(graphicsHandle);

            PInvoke.BitBlt(paintHandle, 0, 0, clientRect.Right - clientRect.Left, clientRect.Bottom - clientRect.Top, memoryDC, 0, 0, PInvoke.SRCCOPY);

            _ = PInvoke.SelectObject(memoryDC, oldBitmapHandle);
            _ = PInvoke.DeleteObject(bitmapHandle);
            _ = PInvoke.DeleteDC(memoryDC);
        }
        finally
        {
            PInvoke.EndPaint(hWnd, ref paint);
        }

        return 0;
    }

    private static void DrawMenuBackground(nint graphicsHandle, RECT clientRect, uint color)
    {
        _ = PInvoke.GdipCreateSolidFill(color, out var bgBrush);

        _ = PInvoke.GdipFillRectangle(
            graphicsHandle,
            bgBrush,
            clientRect.Left,
            clientRect.Top,
            clientRect.Right - clientRect.Left,
            clientRect.Bottom - clientRect.Top);

        _ = PInvoke.GdipDeleteBrush(bgBrush);
    }

    private static unsafe void DrawMenuItem(nint graphicsHandle, PopupMenuLayout layout, MenuItem menuItem, nint font, uint backgroundColor, uint textColor, POINTF* checkBoxPoints, POINTF* submenuArrowPoints)
    {
        var itemRect = menuItem.HitBox;

        _ = PInvoke.GdipCreateSolidFill(backgroundColor, out var backgroundBrush);
        _ = PInvoke.GdipFillRectangle(graphicsHandle, backgroundBrush, itemRect.X, itemRect.Y, itemRect.Width, itemRect.Height);
        _ = PInvoke.GdipDeleteBrush(backgroundBrush);

        var centerY = itemRect.Y + itemRect.Height * 0.5f;

        if (menuItem.IsChecked.GetValueOrDefault()) DrawCheckBox(graphicsHandle, layout, textColor, itemRect, centerY, checkBoxPoints);

        var textLeft = itemRect.X + layout.CheckBoxWidthPx + layout.TextPaddingPx;
        var textRight = itemRect.X + itemRect.Width - layout.SubmenuArrowWidthPx - layout.TextPaddingPx;
        var textRect = new RECTF
        {
            X = textLeft,
            Y = itemRect.Y,
            Width = textRight - textLeft,
            Height = itemRect.Height
        };
        DrawText(graphicsHandle, menuItem.Text, font, textColor, textRect);

        if (menuItem.HasSubMenu) DrawSubmenuArrow(graphicsHandle, layout, textColor, itemRect, centerY, submenuArrowPoints);
    }

    private static void DrawSeparatorItem(nint graphicsHandle, PopupMenuLayout layout, SeparatorItem separatorItem, RECTF itemRect)
    {
        _ = PInvoke.GdipCreateSolidFill(separatorItem.BackgroundColor.ToGdiPlus(), out var backgroundBrush);
        _ = PInvoke.GdipFillRectangle(graphicsHandle, backgroundBrush, itemRect.X, itemRect.Y, itemRect.Width, itemRect.Height);
        _ = PInvoke.GdipDeleteBrush(backgroundBrush);

        var y = itemRect.Y + itemRect.Height * 0.5f;
        _ = PInvoke.GdipCreatePen1(separatorItem.LineColor.ToGdiPlus(), separatorItem.LineThickness * layout.Scale, PInvoke.UnitPixel, out var pen);
        _ = PInvoke.GdipDrawLine(graphicsHandle, pen, itemRect.X + layout.SeparatorPaddingPx, y, itemRect.X + itemRect.Width - layout.SeparatorPaddingPx, y);
        _ = PInvoke.GdipDeletePen(pen);
    }

    private static unsafe void DrawCheckBox(nint graphicsHandle, PopupMenuLayout layout, uint color, RECTF itemRect, float centerY, POINTF* points)
    {
        var checkX = itemRect.X + layout.TextPaddingPx * 0.65f;
        var checkY = centerY - layout.CheckBoxWidthPx * 0.5f;

        points[0] = new POINTF { X = checkX + layout.CheckBoxWidthPx * 0.10f, Y = checkY + layout.CheckBoxWidthPx * 0.55f };
        points[1] = new POINTF { X = checkX + layout.CheckBoxWidthPx * 0.40f, Y = checkY + layout.CheckBoxWidthPx * 0.85f };
        points[2] = new POINTF { X = checkX + layout.CheckBoxWidthPx * 0.90f, Y = checkY + layout.CheckBoxWidthPx * 0.15f };

        _ = PInvoke.GdipCreatePen1(color, 3f * layout.Scale, PInvoke.UnitPixel, out var pen);
        _ = PInvoke.GdipDrawLines(graphicsHandle, pen, points, CheckBoxPoints);
        _ = PInvoke.GdipDeletePen(pen);
    }

    private static void DrawText(nint graphicsHandle, string text, nint font, uint color, RECTF textRect)
    {
        _ = PInvoke.GdipCreateSolidFill(color, out var textBrush);
        _ = PInvoke.GdipCreateStringFormat(0, 0, out var format);
        _ = PInvoke.GdipSetStringFormatFlags(format, PInvoke.StringFormatFlagsNoWrap);
        _ = PInvoke.GdipSetStringFormatAlign(format, PInvoke.StringAlignmentNear);
        _ = PInvoke.GdipSetStringFormatLineAlign(format, PInvoke.StringAlignmentCenter);

        text = NormalizeText(text);

        _ = PInvoke.GdipDrawString(graphicsHandle, text, text.Length, font, ref textRect, format, textBrush);
        _ = PInvoke.GdipDeleteBrush(textBrush);
        _ = PInvoke.GdipDeleteStringFormat(format);
    }

    private static unsafe void DrawSubmenuArrow(nint graphicsHandle, PopupMenuLayout layout, uint color, RECTF itemRect, float centerY, POINTF* points)
    {
        var arrowX = itemRect.X + itemRect.Width - layout.TextPaddingPx - layout.SubmenuArrowWidthPx * 1.5f;

        points[0] = new POINTF { X = arrowX, Y = centerY - layout.SubmenuArrowHeightPx };
        points[1] = new POINTF { X = arrowX + layout.SubmenuArrowWidthPx, Y = centerY };
        points[2] = new POINTF { X = arrowX, Y = centerY + layout.SubmenuArrowHeightPx };

        _ = PInvoke.GdipCreateSolidFill(color, out var brush);
        _ = PInvoke.GdipFillPolygon(graphicsHandle, brush, points, ArrowPoints, 0);
        _ = PInvoke.GdipDeleteBrush(brush);
    }
}