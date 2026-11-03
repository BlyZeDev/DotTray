namespace DotTray.Direct.Context;

using DotTray.Direct.Internal.Rendering;
using DotTray.Direct.Items;
using DotTray.Direct.Primitives;
using DotTray.Direct.Typography;

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
    /// Measures the size, in DIPs, required to render <paramref name="text"/> with <paramref name="textInfo"/>
    /// </summary>
    /// <param name="text">The text to measure</param>
    /// <param name="textInfo">The text information to measure the text for</param>
    /// <returns><see cref="DimF"/></returns>
    public DimF MeasureText(string text, TextInfo textInfo) => _device.MeasureText(text, textInfo);
}