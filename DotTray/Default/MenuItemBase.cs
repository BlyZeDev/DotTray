namespace DotTray.Default;

using System;

/// <summary>
/// Represents the base for a default Win32 menu item
/// </summary>
public abstract class MenuItemBase : ItemBase
{
    internal event Action? Updated;

    /// <summary>
    /// The displayed text
    /// </summary>
    public string Text
    {
        get;
        set
        {
            if (StringComparer.Ordinal.Equals(field, value)) return;

            field = value;
            Update();
        }
    }

    /// <summary>
    /// <see langword="true"/> if this instance is disabled, otherwise <see langword="false"/>
    /// </summary>
    public bool IsDisabled
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    }

    internal MenuItemBase()
    {
        Text = "";
        IsDisabled = false;
    }

    internal abstract void RaiseClick();

    /// <summary>
    /// Notifies that this instance is updated
    /// </summary>
    protected void Update() => Updated?.Invoke();
}

/// <summary>
/// Represents the base for a default Win32 menu item
/// </summary>
public abstract class MenuItemBase<T> : MenuItemBase where T : MenuItemBase<T>
{
    /// <summary>
    /// The action that is invoked if this instance is clicked
    /// </summary>
    public Action<T>? Clicked { get; set; }

    internal MenuItemBase() : base() { }

    internal override void RaiseClick() => Clicked?.Invoke((T)this);
}