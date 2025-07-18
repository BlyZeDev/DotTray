namespace DotTray;

public sealed record SeparatorItem : IMenuItem
{
    public static readonly SeparatorItem Instance = new SeparatorItem();
}