namespace DotTray.Direct;

using System.Drawing;

/// <summary>
/// Holds the style values shared by every popup of a <see cref="PopupMenuTree"/>
/// </summary>
public sealed class PopupMenuStyle
{
    /// <summary>
    /// The background color of the popup windows
    /// </summary>
    public Color Background { get; set; } = SystemColors.Menu;
}