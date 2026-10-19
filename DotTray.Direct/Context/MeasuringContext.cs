namespace DotTray.Direct.Context;

using DotTray.Direct;
using DotTray.Direct.Internal.Rendering;
using DotTray.Direct.Items;
using DotTray.Direct.Primitives;

/// <summary>
/// Includes data for measuring <see cref="MenuItemBase"/> instances
/// </summary>
public sealed class MeasuringContext : Context
{
    private readonly DirectDevice _device;

    internal MeasuringContext(DirectDevice device, float scale) : base(scale)
    {
        _device = device;
    }

    /// <summary>
    /// Measures the size, in DIPs, required to render <paramref name="text"/> with <paramref name="fontInfo"/>
    /// </summary>
    /// <param name="text">The text to measure</param>
    /// <param name="fontInfo">The font information to measure the text in</param>
    /// <returns><see cref="DimF"/></returns>
    public DimF MeasureText(string text, FontInfo fontInfo) => _device.MeasureText(text, fontInfo);
}