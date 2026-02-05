namespace DotTray;

/// <summary>
/// Represents a menu item that can be hovered over
/// </summary>
public interface IHoverable
{
    /// <summary>
    /// This method should return <see langword="true"/> if this <see cref="IHoverable"/> is currently being hovered over, otherwise <see langword="false"/>
    /// </summary>
    /// <param name="x">The x-coordinate</param>
    /// <param name="y">The y-coordinate</param>
    /// <returns></returns>
    public bool IsHovered(int x, int y);
}