namespace DotTray.Internal;

using DotTray.Internal.Native;

internal sealed partial class PopupMenu
{
    private const string FontFamilyName = "Segoe UI Emoji";
    private const int ArrowPoints = 3;
    private const int CheckBoxPoints = 3;

    private static readonly nint _arrowCursor;
    private static readonly nint _handCursor;

    static PopupMenu()
    {
        _arrowCursor = PInvoke.LoadCursor(nint.Zero, PInvoke.IDC_ARROW);
        _handCursor = PInvoke.LoadCursor(nint.Zero, PInvoke.IDC_HAND);
    }
}