namespace DotTray.Internal;

internal readonly record struct PopupMenuLayout
{
    private const float BaseDpi = 96f;

    private readonly float _fontSizeDip;

    public readonly float Scale { get; }

    public readonly float FontSizePx => _fontSizeDip * Scale;
    public readonly float TextPaddingPx => FontSizePx * 0.5f;
    public readonly float CheckBoxWidthPx => FontSizePx;
    public readonly float SubmenuArrowWidthPx => FontSizePx * 0.5f;
    public readonly float SubmenuArrowHeightPx => SubmenuArrowWidthPx * 0.5f;
    public readonly float SeparatorPaddingPx => (CheckBoxWidthPx + TextPaddingPx + SubmenuArrowWidthPx) / 4f;

    public PopupMenuLayout(float fontSizeDip, uint dpi)
    {
        _fontSizeDip = fontSizeDip;
        Scale = dpi / BaseDpi;
    }
}