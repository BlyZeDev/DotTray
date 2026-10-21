namespace DotTray.Direct.Coloring;

using DotTray.Direct.Context;
using DotTray.Direct.Primitives;

/// <summary>
/// Describes a way to create a direct brush
/// </summary>
public interface IDirectBrush
{
    /// <summary>
    /// Creates the brush used to paint inside <paramref name="bounds"/>
    /// </summary>
    /// <param name="context">The context of the window the brush is created for</param>
    /// <param name="bounds">The bounds that will be painted in DIPs</param>
    /// <returns><see cref="DirectBrush"/></returns>
    DirectBrush Create(DirectBrushContext context, Rect bounds);
}