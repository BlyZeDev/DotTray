namespace DotTray.Direct.Internal;

using DotTray.Direct.Internal.Models;
using DotTray.Direct.Primitives;
using System.Runtime.InteropServices;

internal static class PopupPlacement
{
    public static Rect GetWorkArea(Rect anchor)
    {
        var hMonitor = PInvoke.MonitorFromPoint(new POINT
        {
            X = anchor.X,
            Y = anchor.Y
        }, PInvoke.MONITOR_DEFAULTTONEAREST);

        var monitorInfo = new MONITORINFO
        {
            cbSize = (uint)Marshal.SizeOf<MONITORINFO>()
        };
        PInvoke.GetMonitorInfo(hMonitor, ref monitorInfo);

        return new Rect(monitorInfo.rcWork.Left, monitorInfo.rcWork.Top, monitorInfo.rcWork.Right - monitorInfo.rcWork.Left, monitorInfo.rcWork.Bottom - monitorInfo.rcWork.Top);
    }

    public static Rect GetCursorAnchor()
    {
        PInvoke.GetCursorPos(out var pos);
        return new Rect(pos.X, pos.Y, 0, 0);
    }

    public static Pos ResolveRoot(Rect anchor, int width, int height, Rect workArea)
    {
        var x = anchor.X;
        var y = anchor.Y;

        if (x + width > workArea.Right) x = anchor.X - width;
        if (y + height > workArea.Bottom) y = anchor.Y - height;

        return ClampToWorkArea(x, y, width, height, workArea);
    }

    public static Pos ResolveSubmenu(Rect anchor, int width, int height, Rect workArea, IReadOnlyCollection<Rect> obstacles)
    {
        var y = anchor.Top;
        if (y + height > workArea.Bottom) y = anchor.Bottom - height;
        if (y < workArea.Top) y = workArea.Top;

        var rightX = anchor.Right;
        var leftX = anchor.Left - width;

        var rightFits = rightX + width <= workArea.Right;
        var leftFits = leftX >= workArea.Left;

        var rightRect = new Rect(rightX, y, width, height);
        var leftRect = new Rect(leftX, y, width, height);

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

    private static int CountOverlaps(Rect rect, IReadOnlyCollection<Rect> obstacles)
    {
        var count = 0;
        foreach (var obstacle in obstacles)
        {
            if (rect.Left < obstacle.Right && rect.Right > obstacle.Left && rect.Top < obstacle.Bottom && rect.Bottom > obstacle.Top) count++;
        }

        return count;
    }

    private static Pos ClampToWorkArea(int x, int y, int width, int height, Rect workArea)
    {
        x = Math.Clamp(x, workArea.Left, Math.Max(workArea.Left, workArea.Right - width));
        y = Math.Clamp(y, workArea.Top, Math.Max(workArea.Top, workArea.Bottom - height));

        return new Pos(x, y);
    }
}