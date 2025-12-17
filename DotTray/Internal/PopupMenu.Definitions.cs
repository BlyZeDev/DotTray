namespace DotTray.Internal;

using DotTray.Internal.Native;

internal sealed partial class PopupMenu
{
    private const int ScreenMargin = 10;

    private const float FontSize = 16f;

    private const int CheckBoxPoints = 3;
    private const int CheckBoxWidth = 16;
    private const int TextPadding = 8;
    private const int ArrowPoints = 3;
    private const int SubmenuArrowWidth = 8;
    private const int SeparatorPadding = (CheckBoxWidth + TextPadding + SubmenuArrowWidth) / 4;

    private static readonly nint _arrowCursor;
    private static readonly nint _handCursor;

    static PopupMenu()
    {
        _arrowCursor = PInvoke.LoadCursor(nint.Zero, PInvoke.IDC_ARROW);
        _handCursor = PInvoke.LoadCursor(nint.Zero, PInvoke.IDC_HAND);
    }
}