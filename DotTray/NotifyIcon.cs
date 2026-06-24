namespace DotTray;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Represents a Notification Icon that is displayed in the Taskbar
/// </summary>
/// <remarks> 
/// To get the best possible result it's recommended that the icon includes a 16x16 or 32x32 variant with a 32-bit color depth including alpha channel.<br/>
/// Using other sizes or color depths may lead to unexpected results or poor quality rendering.
/// </remarks>
public sealed partial class NotifyIcon : IDisposable
{
    /// <summary>
    /// The unique identifier of this <see cref="NotifyIcon"/> instance
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// The underlying window handle used by this <see cref="NotifyIcon"/> instance
    /// </summary>
    /// <remarks>
    /// This can be used as owner handle for any child windows
    /// </remarks>
    public nint Handle => hWnd;

    /// <summary>
    /// The <see cref="MenuItemCollection"/> of this <see cref="NotifyIcon"/> instance
    /// </summary>
    public MenuItemCollection MenuItems { get; }

    /// <summary>
    /// The tooltip text of this <see cref="NotifyIcon"/> instance, or <see langword="null"/> if no tooltip should be shown
    /// </summary>
    /// <remarks>
    /// The default value is <see langword="null"/>
    /// </remarks>
    public string? ToolTip { get; private set; }

    /// <summary>
    /// The current visibility of this <see cref="NotifyIcon"/>.<br/>
    /// <see langword="true"/> if it is visible, otherwise <see langword="false"/>
    /// </summary>
    /// <remarks>
    /// The default value is <see langword="true"/>
    /// </remarks>
    public bool IsVisible { get; private set; }

    /// <summary>
    /// Fires whenever the user interacts with the <see cref="NotifyIcon"/> or a <see cref="BalloonNotification"/>.
    /// </summary>
    /// <remarks>
    /// Note: This event is raised on the <see cref="NotifyIcon"/>'s background STA thread.
    /// </remarks>
    public event Action<NotifyIconInteractedEventArgs>? Interacted;

    /// <summary>
    /// Sets the <see cref="ToolTip"/> for this <see cref="NotifyIcon"/> instance
    /// </summary>
    /// <remarks>
    /// <paramref name="toolTip"/> is truncated to fit into the allowed Windows tooltip character length
    /// </remarks>
    /// <param name="toolTip">The text to set as <see cref="ToolTip"/></param>
    public void SetToolTip(string? toolTip)
    {
        if (ToolTip is null && toolTip is null) return;

        if (toolTip is not null)
        {
            toolTip = toolTip.Length > NOTIFYICONDATA.SZTIP_LENGTH ? toolTip[..NOTIFYICONDATA.SZTIP_LENGTH] : toolTip;

            if (ToolTip?.Equals(toolTip, StringComparison.Ordinal) ?? false) return;
        }

        ToolTip = toolTip;
        var success = PInvoke.PostMessage(hWnd, WM_APP_TRAYICON_TOOLTIP, 0, 0);
        NotifyIconException.ThrowIfFalse(success, "Posting a tooltip message failed");
    }

    /// <summary>
    /// Hides this <see cref="NotifyIcon"/> instance
    /// </summary>
    public void Hide()
    {
        if (!IsVisible) return;

        IsVisible = false;
        var success = PInvoke.PostMessage(hWnd, WM_APP_TRAYICON_VISIBILITY, 0, 0);
        NotifyIconException.ThrowIfFalse(success, "Posting a visibility message failed");
    }

    /// <summary>
    /// Shows this <see cref="NotifyIcon"/> instance
    /// </summary>
    public void Show()
    {
        if (IsVisible) return;

        IsVisible = true;
        var success = PInvoke.PostMessage(hWnd, WM_APP_TRAYICON_VISIBILITY, 0, 0);
        NotifyIconException.ThrowIfFalse(success, "Posting a visibility message failed");
    }

    /// <summary>
    /// Shows a balloon notification
    /// </summary>
    /// <param name="balloon">The balloon notification to show</param>
    public void ShowBalloon(BalloonNotification balloon)
    {
        nextBalloon = balloon with
        {
            Title = balloon.Title.Length > NOTIFYICONDATA.SZINFOTITLE_LENGTH ? balloon.Title[..NOTIFYICONDATA.SZINFOTITLE_LENGTH] : balloon.Title,
            Message = balloon.Message.Length > NOTIFYICONDATA.SZINFO_LENGTH ? balloon.Message[..NOTIFYICONDATA.SZINFO_LENGTH] : balloon.Message
        };

        var success = PInvoke.PostMessage(hWnd, WM_APP_TRAYICON_BALLOON, 0, 0);
        NotifyIconException.ThrowIfFalse(success, "Posting a balloon message failed");
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        var success = PInvoke.PostMessage(hWnd, PInvoke.WM_CLOSE, 0, 0);
        NotifyIconException.ThrowIfFalse(success, "Posting a close message failed");

        if (_thread.IsAlive) _thread.Join();

        totalIcons--;
        if (totalIcons > 0 || gdipToken == nint.Zero) return;

        PInvoke.GdiplusShutdown(gdipToken);
        gdipToken = nint.Zero;
    }

    /// <summary>
    /// Creates and runs a <see cref="NotifyIcon"/> instance synchronously
    /// </summary>
    /// <remarks>
    /// This will block until the <see cref="NotifyIcon"/> instance is ready or an <see cref="Exception"/> occurs.<br/>
    /// When using an icon handle as <paramref name="source"/> it will not be destroyed, the responsibility lies with the caller
    /// </remarks>
    /// <param name="source">The source of the icon to use</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to stop this <see cref="NotifyIcon"/> instance</param>
    /// <param name="handler">The handler to use for interaction with this <see cref="NotifyIcon"/> instance</param>
    /// <returns><see cref="NotifyIcon"/></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="NotifyIconException"></exception>
    public static NotifyIcon Run(IconSource source, CancellationToken cancellationToken, INotifyIconHandler? handler = null) => RunInternal(PrepareIconHandle(source), handler, cancellationToken);

    /// <summary>
    /// Creates and runs a <see cref="NotifyIcon"/> instance synchronously
    /// </summary>
    /// <remarks>
    /// This will block until the <see cref="NotifyIcon"/> instance is ready or an <see cref="Exception"/> occurs.<br/>
    /// When using an icon handle as <paramref name="source"/> it will not be destroyed, the responsibility lies with the caller
    /// </remarks>
    /// <param name="source">The source of the icon to use</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to stop this <see cref="NotifyIcon"/> instance</param>
    /// <param name="handler">The handler to use for interaction with this <see cref="NotifyIcon"/> instance</param>
    /// <returns><see cref="NotifyIcon"/></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="NotifyIconException"></exception>
    public static Task<NotifyIcon> RunAsync(IconSource source, CancellationToken cancellationToken, INotifyIconHandler? handler = null) => RunInternalAsync(PrepareIconHandle(source), handler, cancellationToken);
}