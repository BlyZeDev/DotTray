namespace DotTray.Direct.Internal.Popup;

using DotTray.Direct.Internal.Rendering;
using DotTray.Direct.Primitives;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Request = (Primitives.Pos Anchor, nint Owner);
using DispatcherInfo = (System.Runtime.InteropServices.GCHandle Handle, nint HWnd, bool IsAlive);

internal sealed class PopupThread : IDisposable
{
    internal const uint WM_APP_OPEN = PInvoke.WM_APP + 0x2100;
    internal const uint WM_APP_CLOSE = PInvoke.WM_APP + 0x2101;
    internal const uint WM_APP_FORCEUPDATE = PInvoke.WM_APP + 0x2102;
    internal const uint WM_APP_DISPOSE = PInvoke.WM_APP + 0x2103;

    [ThreadStatic]
    private static PopupThread? current;

    private readonly Thread _thread;
    private readonly ManualResetEventSlim _ready = new ManualResetEventSlim(false);
    private readonly ConcurrentQueue<Request> _openRequests = new ConcurrentQueue<Request>();

    private DispatcherInfo dispatcher;
    private PopupSession? session;
    private DirectDevice? device;
    private List<Exception>? failures;
    private Exception? startupFailure;
    private int disposed;

    public DirectPopupMenuHandler Handler { get; }

    public DirectDevice Device => device ?? throw new InvalidOperationException("The popup thread has no device");

    public bool IsOpen
    {
        get
        {
            var active = Volatile.Read(ref session);
            return active is not null && active.IsActive;
        }
    }

    public PopupThread(DirectPopupMenuHandler handler)
    {
        Handler = handler;

        _thread = new Thread(Run)
        {
            IsBackground = true,
            Name = $"{nameof(DotTray)}.{nameof(Direct)} Popup"
        };
        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();

        _ready.Wait();

        if (startupFailure is not null) ExceptionDispatchInfo.Throw(startupFailure);
    }

    public static void Fail(Exception exception)
    {
        if (current is null) ExceptionDispatchInfo.Throw(exception);

        current.RecordFailure(exception);
    }

    public void Open(Pos anchor, nint ownerHWnd)
    {
        if (Volatile.Read(ref disposed) != 0) return;

        _openRequests.Enqueue((anchor, ownerHWnd));
        Post(WM_APP_OPEN, nint.Zero, nint.Zero);
    }

    public void Close() => Post(WM_APP_CLOSE, nint.Zero, nint.Zero);

    public void ForceUpdate() => Post(WM_APP_FORCEUPDATE, nint.Zero, nint.Zero);

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0) return;

        var hWnd = Volatile.Read(ref dispatcher.HWnd);
        if (hWnd != nint.Zero) PInvoke.PostMessage(hWnd, WM_APP_DISPOSE, nint.Zero, nint.Zero);

        if (Thread.CurrentThread != _thread) _thread.Join(TimeSpan.FromSeconds(5));
    }

    public void OnSessionEnded(PopupSession ended) => Interlocked.CompareExchange(ref session, null, ended);

    public nint HandleDispatcherMessage(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case WM_APP_OPEN: OpenLatest(); return 0;
            case WM_APP_CLOSE: session?.Dismiss(); return 0;
            case WM_APP_FORCEUPDATE: session?.RequestRelayoutAll(); return 0;

            case WM_APP_DISPOSE:
                session?.Abort();
                PInvoke.DestroyWindow(hWnd);
                return 0;

            case PInvoke.WM_DESTROY: PInvoke.PostQuitMessage(0); return 0;

            case PInvoke.WM_NCDESTROY:
                dispatcher.IsAlive = false;
                PInvoke.SetWindowLongPtr(hWnd, PInvoke.GWLP_USERDATA, nint.Zero);
                if (dispatcher.Handle.IsAllocated) dispatcher.Handle.Free();
                return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private void Post(uint msg, nint wParam, nint lParam)
    {
        if (Volatile.Read(ref disposed) != 0) return;

        var hWnd = Volatile.Read(ref dispatcher.HWnd);
        if (hWnd != nint.Zero) PInvoke.PostMessage(hWnd, msg, wParam, lParam);
    }

    private void OpenLatest()
    {
        Request? latest = null;

        while (_openRequests.TryDequeue(out var request))
        {
            latest = request;
        }

        if (latest.HasValue) OpenCore(latest.Value.Anchor, latest.Value.Owner);
    }

    private void OpenCore(Pos anchor, nint ownerHWnd)
    {
        if (Volatile.Read(ref disposed) != 0) return;

        session?.Abort();

        if (Handler.MenuItems.IsEmpty) return;

        if (device is null || device.IsLost)
        {
            device?.Dispose();
            device = new DirectDevice();
        }

        var opened = new PopupSession(this);
        Volatile.Write(ref session, opened);

        opened.TryOpen(new Rect(anchor.X, anchor.Y, 0, 0), ownerHWnd);
    }

    private void Run()
    {
        current = this;

        try
        {
            PInvoke.SetThreadDpiAwarenessContext(PInvoke.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);

            dispatcher.Handle = GCHandle.Alloc(this);
            var hWnd = PopupWindowClass.CreateDispatcherWindow(GCHandle.ToIntPtr(dispatcher.Handle));
            dispatcher.IsAlive = true;
            dispatcher.HWnd = hWnd;

            device = new DirectDevice();

            Volatile.Write(ref dispatcher.HWnd, hWnd);
        }
        catch (Exception ex)
        {
            startupFailure = ex;
            Teardown();
            _ready.Set();
            return;
        }

        _ready.Set();

        try
        {
            while (PInvoke.GetMessage(out var message, nint.Zero, 0, 0))
            {
                PInvoke.TranslateMessage(ref message);
                PInvoke.DispatchMessage(ref message);

                ThrowIfFailed();
            }
        }
        finally
        {
            Teardown();
        }

        ThrowIfFailed();
    }

    private void RecordFailure(Exception exception)
    {
        var list = failures ??= [];
        var first = list.Count == 0;
        list.Add(exception);

        if (!first) return;

        try
        {
            session?.Abort();
        }
        catch (Exception inner)
        {
            list.Add(inner);
        }
    }

    private void ThrowIfFailed()
    {
        if (failures is not { Count: > 0 }) return;

        var all = failures;
        failures = null;

        if (all.Count == 1) ExceptionDispatchInfo.Throw(all[0]);

        throw new AggregateException(all);
    }

    private void Teardown()
    {
        try
        {
            session?.Abort();
        }
        catch (Exception ex)
        {
            failures ??= [];
            failures.Add(ex);
        }

        if (dispatcher.IsAlive)
        {
            dispatcher.IsAlive = false;
            PInvoke.DestroyWindow(dispatcher.HWnd);
        }

        device?.Dispose();
        device = null;

        current = null;
    }
}