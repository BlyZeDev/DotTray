namespace DotTray;

/// <summary>
/// Represents a <see cref="NotifyIcon"/> separator item
/// </summary>
public sealed class SeparatorItem : MenuItemBase
{
    internal override float Height => LineThickness * 2f;

    /// <summary>
    /// The background color of this separator
    /// </summary>
    public TrayColor BackgroundColor
    {
        get => field;
        set
        {
            if (field == value) return;

            field = value;
            Update();
        }
    }

    /// <summary>
    /// The line color of this separator
    /// </summary>
    public TrayColor LineColor
    {
        get => field;
        set
        {
            if (field == value) return;

            field = value;
            Update();
        }
    }

    /// <summary>
    /// The line thickness of this separator
    /// </summary>
    public float LineThickness
    {
        get => field;
        set
        {
            if (field == value) return;

            field = value;
            Update();
        }
    }

    internal SeparatorItem()
    {

    }
}