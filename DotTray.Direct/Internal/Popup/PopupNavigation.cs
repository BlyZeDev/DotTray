namespace DotTray.Direct.Internal.Popup;

internal static class PopupNavigation
{
    public static int First(ReadOnlySpan<ItemLayout> items)
    {
        for (var i = 0; i < items.Length; i++)
        {
            if (!items[i].Item.IgnoreInteraction) return i;
        }

        return -1;
    }

    public static int Last(ReadOnlySpan<ItemLayout> items)
    {
        for (var i = items.Length - 1; i >= 0; i--)
        {
            if (!items[i].Item.IgnoreInteraction) return i;
        }

        return -1;
    }

    public static int Step(ReadOnlySpan<ItemLayout> items, int current, int direction)
    {
        var length = items.Length;
        if (length == 0) return -1;

        if (current < 0) return direction > 0 ? First(items) : Last(items);

        for (var step = 1; step <= length; step++)
        {
            var index = (((current + direction * step) % length) + length) % length;

            if (!items[index].Item.IgnoreInteraction) return index;
        }

        return -1;
    }
}