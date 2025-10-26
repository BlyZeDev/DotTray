namespace DotTray;

using DotTray.Internal;
using System;

/// <summary>
/// Represents a <see cref="NotifyIcon"/> separator item
/// </summary>
public sealed class SeparatorItem : IMenuItem
{
    private static readonly float DefaultLineThickness = 1f;

    private TrayColor backgroundColor;
    private TrayColor lineColor;
    private float lineThickness;

    private event Action? updated;
    event Action? IMenuItem.Updated
    {
        add => updated += value;
        remove => updated -= value;
    }

    /// <summary>
    /// The background color of this separator
    /// </summary>
    public TrayColor BackgroundColor
    {
        get => backgroundColor;
        set
        {
            if (backgroundColor == value) return;

            backgroundColor = value;
            Update();
        }
    }

    /// <summary>
    /// The line color of this separator
    /// </summary>
    public TrayColor LineColor
    {
        get => lineColor;
        set
        {
            if (lineColor == value) return;

            lineColor = value;
            Update();
        }
    }

    /// <summary>
    /// The line thickness of this separator
    /// </summary>
    public float LineThickness
    {
        get => lineThickness;
        set
        {
            if (lineThickness == value) return;

            lineThickness = value;
            Update();
        }
    }

    internal SeparatorItem()
    {
        backgroundColor = DefaultColors.SeparatorBackgroundColor;
        lineColor = DefaultColors.SeparatorLineColor;
        lineThickness = DefaultLineThickness;
    }

    private void Update() => updated?.Invoke();
}