namespace DotTray.Direct;

using DotTray.Direct.Coloring;
using DotTray.Direct.Items;

/// <summary>
/// Holds the style values used by default for everything from <see cref="DirectPopupMenuHandler"/>
/// </summary>
public static class DirectDefaultStyles
{
    /// <summary>
    /// The default popup window background used for <see cref="DirectPopupMenuHandler.Background"/>
    /// </summary>
    public static IDirectBrush PopupWindowBackground { get; set; } = new SolidColorBrush(new Rgba(32, 32, 32));

    /// <summary>
    /// The default item background used for menu items, eg. <see cref="MenuItem.Background"/>
    /// </summary>
    public static IDirectBrush ItemBackground { get; set; } = SolidColorBrush.Transparent;

    /// <summary>
    /// The default item foreground used for menu items, eg. <see cref="MenuItem.Foreground"/>
    /// </summary>
    public static IDirectBrush ItemForeground { get; set; } = new SolidColorBrush(new Rgba(243, 243, 243));

    /// <summary>
    /// The default item background hover used for menu items, eg. <see cref="MenuItem.BackgroundHover"/>
    /// </summary>
    public static IDirectBrush ItemBackgroundHover { get; set; } = new SolidColorBrush(new Rgba(51, 51, 51));

    /// <summary>
    /// The default item foreground hover used for menu items, eg. <see cref="MenuItem.ForegroundHover"/>
    /// </summary>
    public static IDirectBrush ItemForegroundHover { get; set; } = new SolidColorBrush(new Rgba(255, 255, 255));

    /// <summary>
    /// The default item background disabled used for menu items, eg. <see cref="MenuItem.BackgroundDisabled"/>
    /// </summary>
    public static IDirectBrush ItemBackgroundDisabled { get; set; } = SolidColorBrush.Transparent;

    /// <summary>
    /// The default item foreground disabled used for menu items, eg. <see cref="MenuItem.ForegroundDisabled"/>
    /// </summary>
    public static IDirectBrush ItemForegroundDisabled { get; set; } = new SolidColorBrush(new Rgba(118, 118, 118));
}