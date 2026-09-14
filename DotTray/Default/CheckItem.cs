namespace DotTray.Default;

/// <summary>
/// Represents a default Win32 menu item that can be checked
/// </summary>
public sealed class CheckItem : MenuItemBase<CheckItem>
{
    /// <summary>
    /// <see langword="true"/> if this instance is checked, otherwise <see langword="false"/>
    /// </summary>
    public bool IsChecked
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    }

    /// <summary>
    /// Initializes a <see cref="CheckItem"/> instance with the default configuration
    /// </summary>
    public CheckItem() : base()
    {
        IsChecked = false;
    }
}