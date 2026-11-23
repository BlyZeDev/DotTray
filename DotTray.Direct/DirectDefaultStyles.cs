namespace DotTray.Direct;

using DotTray.Direct.Coloring;
using DotTray.Direct.Internal;
using DotTray.Direct.Items;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Holds the style values used by default for everything from <see cref="DirectPopupMenuHandler"/>
/// </summary>
public static class DirectDefaultStyles
{
    /// <summary>
    /// The default popup window background used for <see cref="DirectPopupMenuHandler.Background"/>
    /// </summary>
    public static IDirectBrush PopupWindowBackground { get; set; }

    /// <summary>
    /// The default item background used for menu items, eg. <see cref="MenuItem.Background"/>
    /// </summary>
    public static IDirectBrush ItemBackground { get; set; }

    /// <summary>
    /// The default item foreground used for menu items, eg. <see cref="MenuItem.Foreground"/>
    /// </summary>
    public static IDirectBrush ItemForeground { get; set; }

    /// <summary>
    /// The default item background hover used for menu items, eg. <see cref="MenuItem.BackgroundHover"/>
    /// </summary>
    public static IDirectBrush ItemBackgroundHover { get; set; }

    /// <summary>
    /// The default item foreground hover used for menu items, eg. <see cref="MenuItem.ForegroundHover"/>
    /// </summary>
    public static IDirectBrush ItemForegroundHover { get; set; }

    /// <summary>
    /// The default item background disabled used for menu items, eg. <see cref="MenuItem.BackgroundDisabled"/>
    /// </summary>
    public static IDirectBrush ItemBackgroundDisabled { get; set; }

    /// <summary>
    /// The default item foreground disabled used for menu items, eg. <see cref="MenuItem.ForegroundDisabled"/>
    /// </summary>
    public static IDirectBrush ItemForegroundDisabled { get; set; }

    static DirectDefaultStyles() => UseSystem();

    /// <summary>
    /// Overrides all properties by calling <see cref="UseLight"/> or <see cref="UseDark"/> depending on the system theme
    /// </summary>
    [MemberNotNull(
        nameof(PopupWindowBackground),
        nameof(ItemBackground),
        nameof(ItemForeground),
        nameof(ItemBackgroundHover),
        nameof(ItemForegroundHover),
        nameof(ItemBackgroundDisabled),
        nameof(ItemForegroundDisabled))]
    public static void UseSystem()
    {
        if (PInvoke.ShouldSystemUseDarkMode()) UseDark();
        else UseLight();
    }

    /// <summary>
    /// Overrides all properties to the light theme
    /// </summary>
    [MemberNotNull(
        nameof(PopupWindowBackground),
        nameof(ItemBackground),
        nameof(ItemForeground),
        nameof(ItemBackgroundHover),
        nameof(ItemForegroundHover),
        nameof(ItemBackgroundDisabled),
        nameof(ItemForegroundDisabled))]
    public static void UseLight()
    {
        PopupWindowBackground = new SolidColorBrush(new Rgba(249, 249, 249));

        ItemBackground = SolidColorBrush.Transparent;
        ItemForeground = new SolidColorBrush(new Rgba(31, 31, 31));

        ItemBackgroundHover = new SolidColorBrush(new Rgba(229, 229, 229));
        ItemForegroundHover = new SolidColorBrush(new Rgba(31, 31, 31));

        ItemBackgroundDisabled = SolidColorBrush.Transparent;
        ItemForegroundDisabled = new SolidColorBrush(new Rgba(160, 160, 160));
    }

    /// <summary>
    /// Overrides all properties to the dark theme
    /// </summary>
    [MemberNotNull(
        nameof(PopupWindowBackground),
        nameof(ItemBackground),
        nameof(ItemForeground),
        nameof(ItemBackgroundHover),
        nameof(ItemForegroundHover),
        nameof(ItemBackgroundDisabled),
        nameof(ItemForegroundDisabled))]
    public static void UseDark()
    {
        PopupWindowBackground = new SolidColorBrush(new Rgba(32, 32, 32));

        ItemBackground = SolidColorBrush.Transparent;
        ItemForeground = new SolidColorBrush(new Rgba(243, 243, 243));

        ItemBackgroundHover = new SolidColorBrush(new Rgba(51, 51, 51));
        ItemForegroundHover = new SolidColorBrush(new Rgba(255, 255, 255));

        ItemBackgroundDisabled = SolidColorBrush.Transparent;
        ItemForegroundDisabled = new SolidColorBrush(new Rgba(118, 118, 118));
    }
}