namespace DotTray.Internal;

internal readonly record struct PopupMenuLayout
{
    private const float BaseDpi = 96f;
    private const float BaseFontSizeDip = 24f;

    public readonly float Scale { get; }

    public readonly float FontSizePx { get; }
    public readonly float TextPaddingPx { get; }
    public readonly float CheckBoxWidthPx { get; }
    public readonly float SubmenuArrowWidthPx { get; }
    public readonly float SubmenuArrowHeightPx { get; }
    public readonly float SeparatorPaddingPx { get; }

    public PopupMenuLayout(uint dpi)
    {
        Scale = dpi / BaseDpi;

        FontSizePx = BaseFontSizeDip * Scale;
        TextPaddingPx = FontSizePx * 0.5f;
        CheckBoxWidthPx = FontSizePx;
        SubmenuArrowWidthPx = FontSizePx * 0.5f;
        SubmenuArrowHeightPx = SubmenuArrowWidthPx * 0.5f;

        SeparatorPaddingPx = (CheckBoxWidthPx + TextPaddingPx + SubmenuArrowWidthPx) / 4f;
    }
}