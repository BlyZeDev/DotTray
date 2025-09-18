namespace DotTray;

/// <summary>
/// Represents a <see cref="NotifyIcon"/> checkable menu item
/// </summary>
public sealed record CheckMenuItem : MenuItem
{
    /// <summary>
    /// <see langword="true"/> if this <see cref="CheckMenuItem"/> is checked, otherwise <see langword="false"/>.
    /// <see langword="null"/> if this <see cref="CheckMenuItem"/> is not checkable.<br/>
    /// </summary>
    public bool? IsChecked
    {
        get => isChecked;
        set
        {
            if (isChecked == value) return;

            isChecked = value;
            Changed?.Invoke();
        }
    }

    public CheckMenuItem(string text, bool? isChecked, bool isDisabled, Rgb backgroundColor, Rgb backgroundHoverColor, Rgb backgroundDisabledColor, Rgb textColor, Rgb textHoverColor, Rgb textDisabledColor) : base(text, isChecked, isDisabled, backgroundColor, backgroundHoverColor, backgroundDisabledColor, textColor, textHoverColor, textDisabledColor)
    {
    }
}