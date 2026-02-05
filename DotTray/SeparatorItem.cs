namespace DotTray;

/// <summary>
/// Represents a <see cref="NotifyIcon"/> separator item
/// </summary>
public sealed class SeparatorItem : MenuItemBase
{
    private float HeightMultiplier => LineThickness * 0.1f;

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

    internal SeparatorItem() { }

    /// <inheritdoc/>
    internal protected override void Measure(PopupMenuInfo info, out float width, out float height)
    {
        width = 0;
        height = HeightMultiplier * info.AbsoluteFontSize;
    }

    /// <inheritdoc/>
    internal protected override void Draw(PopupMenuInfo info)
    {

    }
}