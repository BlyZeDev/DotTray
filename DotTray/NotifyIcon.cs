namespace DotTray;

using DotTray.Abstract;
using DotTray.Internal;
using DotTray.Internal.Native;
using DotTray.Popup;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides factory methods for creating Notification Icons
/// </summary>
public static class NotifyIcon
{
    internal static uint TotalIcons;
    internal static nint GdipToken;

    /// <summary>
    /// Creates and runs a <see cref="NotifyIcon{THandler}"/> instance synchronously using <see cref="NativePopupMenuHandler"/>
    /// </summary>
    /// <param name="source">The source of the icon to display</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to stop this <see cref="NotifyIcon{THandler}"/> instance</param>
    /// <returns><see cref="NotifyIcon{THandler}"/></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="NotifyIconException"></exception>
    public static NotifyIcon<NativePopupMenuHandler> Run(IconSource source, CancellationToken cancellationToken)
        => RunInternal(PrepareIconHandle(source), new NativePopupMenuHandler(), cancellationToken);

    /// <summary>
    /// Creates and runs a <see cref="NotifyIcon{THandler}"/> instance synchronously
    /// </summary>
    /// <remarks>
    /// This will block until the <see cref="NotifyIcon{THandler}"/> instance is ready or an <see cref="Exception"/> occurs.<br/>
    /// When using an icon handle as <paramref name="source"/> it will not be destroyed, the responsibility lies with the caller
    /// </remarks>
    /// <param name="source">The source of the icon to display</param>
    /// <param name="handler">The handler to use for interaction with this <see cref="NotifyIcon{THandler}"/> instance</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to stop this <see cref="NotifyIcon{THandler}"/> instance</param>
    /// <returns><see cref="NotifyIcon{THandler}"/></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="NotifyIconException"></exception>
    public static NotifyIcon<THandler> Run<THandler>(IconSource source, THandler handler, CancellationToken cancellationToken)
        where THandler : class, INotifyIconHandler
        => RunInternal(PrepareIconHandle(source), handler, cancellationToken);

    /// <summary>
    /// Creates and runs a <see cref="NotifyIcon{THandler}"/> instance asynchronously using <see cref="NativePopupMenuHandler"/>
    /// </summary>
    /// <param name="source">The source of the icon to display</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to stop this <see cref="NotifyIcon{THandler}"/> instance</param>
    /// <returns><see cref="NotifyIcon{THandler}"/></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="NotifyIconException"></exception>
    public static Task<NotifyIcon<NativePopupMenuHandler>> RunAsync(IconSource source, CancellationToken cancellationToken)
        => RunInternalAsync(PrepareIconHandle(source), new NativePopupMenuHandler(), cancellationToken);

    /// <summary>
    /// Creates and runs a <see cref="NotifyIcon{THandler}"/> instance synchronously
    /// </summary>
    /// <remarks>
    /// This will block until the <see cref="NotifyIcon{THandler}"/> instance is ready or an <see cref="Exception"/> occurs.<br/>
    /// When using an icon handle as <paramref name="source"/> it will not be destroyed, the responsibility lies with the caller
    /// </remarks>
    /// <param name="source">The source of the icon to display</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to stop this <see cref="NotifyIcon{THandler}"/> instance</param>
    /// <param name="handler">The handler to use for interaction with this <see cref="NotifyIcon{THandler}"/> instance</param>
    /// <returns><see cref="NotifyIcon{THandler}"/></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="NotifyIconException"></exception>
    public static Task<NotifyIcon<THandler>> RunAsync<THandler>(IconSource source, THandler handler, CancellationToken cancellationToken)
        where THandler : class, INotifyIconHandler
        => RunInternalAsync(PrepareIconHandle(source), handler, cancellationToken);

    private static NotifyIcon<THandler> RunInternal<THandler>(nint preparedIcoHandle, THandler handler, CancellationToken cancellationToken)
        where THandler : class, INotifyIconHandler
    {
        using (var manualLock = new ManualResetEventSlim(false))
        {
            var icon = new NotifyIcon<THandler>(preparedIcoHandle, handler, manualLock.Set, cancellationToken);

            manualLock.Wait(cancellationToken);

            return icon;
        }
    }

    private static async Task<NotifyIcon<THandler>> RunInternalAsync<THandler>(nint preparedIcoHandle, THandler handler, CancellationToken cancellationToken)
        where THandler : class, INotifyIconHandler
    {
        var manualLock = new AsyncManualResetEvent(false);

        var icon = new NotifyIcon<THandler>(preparedIcoHandle, handler, manualLock.Set, cancellationToken);

        await manualLock.WaitAsync(cancellationToken);

        return icon;
    }

    private static nint PrepareIconHandle(IconSource source)
    {
        if (source.IsPath)
        {
            var path = source.Path;

            if (!Path.GetExtension(path).Equals(".ico", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("The path needs to point to an .ico file", nameof(source));
            if (!File.Exists(path)) throw new FileNotFoundException("The .ico file could not be found", path);

            var handle = PInvoke.LoadImage(nint.Zero, path, PInvoke.IMAGE_ICON, 0, 0, PInvoke.LR_LOADFROMFILE | PInvoke.LR_DEFAULTSIZE);
            NotifyIconException.ThrowIfNull(handle, "The .ico file could not be loaded");
            return handle;
        }
        else if (source.IsHandle)
        {
            var copyHandle = PInvoke.CopyIcon(source.Handle);
            NotifyIconException.ThrowIfNull(copyHandle, "Copying the icon handle failed");
            return copyHandle;
        }
        else throw new ArgumentException("The icon source is invalid", nameof(source));
    }
}