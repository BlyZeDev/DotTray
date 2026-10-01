namespace DotTray.Drawing;

public static class NotifyIconExtensions
{
    extension(NotifyIcon)
    {
        /// <summary>
        /// Creates and runs a <see cref="NotifyIcon{THandler}"/> instance using <see cref="DrawingPopupMenuHandler"/> synchronously
        /// </summary>
        /// <remarks>
        /// This will block until the <see cref="NotifyIcon{THandler}"/> instance is ready or an <see cref="Exception"/> occurs.<br/>
        /// When using an icon handle as <paramref name="source"/> it will not be destroyed, the responsibility lies with the caller
        /// </remarks>
        /// <param name="source">The source of the icon to display</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to stop this <see cref="NotifyIcon{THandler}"/> instance</param>
        /// <returns><see cref="NotifyIcon{THandler}"/></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="NotifyIconException"></exception>
        public static NotifyIcon<DrawingPopupMenuHandler> RunDrawing(IconSource source, CancellationToken cancellationToken)
            => NotifyIcon.Run(source, new DrawingPopupMenuHandler(), cancellationToken);

        /// <summary>
        /// Creates and runs a <see cref="NotifyIcon{THandler}"/> instance using <see cref="DrawingPopupMenuHandler"/> asynchronously
        /// </summary>
        /// <remarks>
        /// When using an icon handle as <paramref name="source"/> it will not be destroyed, the responsibility lies with the caller
        /// </remarks>
        /// <param name="source">The source of the icon to display</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to stop this <see cref="NotifyIcon{THandler}"/> instance</param>
        /// <returns><see cref="NotifyIcon{THandler}"/></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="NotifyIconException"></exception>
        public static Task<NotifyIcon<DrawingPopupMenuHandler>> RunDrawingAsync(IconSource source, CancellationToken cancellationToken)
            => NotifyIcon.RunAsync(source, new DrawingPopupMenuHandler(), cancellationToken);
    }
}