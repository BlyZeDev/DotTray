namespace DotTray.Direct.Context;

using DotTray.Direct.Internal.Rendering;
using DotTray.Direct.Items;

/// <summary>
/// Includes data for measuring <see cref="MenuItemBase"/> instances
/// </summary>
public sealed class MeasuringContext : Context
{
    internal MeasuringContext(DirectSurface surface) : base(surface) { }
}