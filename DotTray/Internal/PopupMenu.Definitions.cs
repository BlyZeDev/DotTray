namespace DotTray.Internal;

using DotTray.Internal.Native;

internal sealed partial class PopupMenu
{
    private const int ScreenMargin = 10;

    private const float FontSize = 24f;

    private const int CheckBoxPoints = 3;
    private const float CheckBoxWidth = FontSize;
    private const float TextPadding = FontSize / 2f;
    private const int ArrowPoints = 3;
    private const float SubmenuArrowWidth = FontSize / 2f;
    private const float SubmenuArrowHeight = SubmenuArrowWidth / 2f;
    private const float SeparatorPadding = (CheckBoxWidth + TextPadding + SubmenuArrowWidth) / 4f;

    private static readonly nint _arrowCursor;
    private static readonly nint _handCursor;

    static PopupMenu()
    {
        _arrowCursor = PInvoke.LoadCursor(nint.Zero, PInvoke.IDC_ARROW);
        _handCursor = PInvoke.LoadCursor(nint.Zero, PInvoke.IDC_HAND);
    }
}