namespace DotTray.Internal;

internal static class DefaultColors
{
    public static readonly TrayColor PopupMenuColor = new TrayColor(40, 40, 40);

    public static readonly TrayColor MenuItemBackgroundColor = TrayColor.Transparent;
    public static readonly TrayColor MenuItemBackgroundHoverColor = new TrayColor(0, 120, 215);
    public static readonly TrayColor MenuItemBackgroundDisabledColor = TrayColor.Gray;
    public static readonly TrayColor MenuItemTextColor = TrayColor.White;
    public static readonly TrayColor MenuItemTextHoverColor = MenuItemTextColor;
    public static readonly TrayColor MenuItemTextDisabledColor = new TrayColor(109, 109, 109);

    public static readonly TrayColor SeparatorBackgroundColor = MenuItemBackgroundColor;
    public static readonly TrayColor SeparatorLineColor = MenuItemTextColor;
}