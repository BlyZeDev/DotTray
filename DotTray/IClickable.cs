namespace DotTray;

/// <summary>
/// Represents a menu item that can be clicked
/// </summary>
public interface IClickable : IHoverable
{
    /// <summary>
    /// This method is called if this <see cref="IClickable"/> instance is clicked
    /// </summary>
    public void OnClick();
}