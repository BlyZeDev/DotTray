namespace DotTray.Internal;

using System.Threading;
using System.Threading.Tasks;

// Original code by AsyncEx library: https://github.com/StephenCleary/AsyncEx/blob/master/src/Nito.AsyncEx.Coordination/AsyncManualResetEvent.cs
// Adapted for my use case

internal sealed class AsyncManualResetEvent
{
    private readonly Lock _lock;
    private TaskCompletionSource<object?> completitionSource;

    public bool IsSet
    {
        get
        {
            lock (_lock)
            {
                return completitionSource.Task.IsCompleted;
            }
        }
    }

    public AsyncManualResetEvent(bool initialState)
    {
        _lock = new Lock();

        completitionSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (initialState) completitionSource.TrySetResult(null);
    }

    public Task WaitAsync()
    {
        lock (_lock)
        {
            return completitionSource.Task;
        }
    }

    public Task WaitAsync(CancellationToken cancellationToken)
    {
        var waitTask = WaitAsync();
        return waitTask.IsCompleted ? waitTask : waitTask.WaitAsync(cancellationToken);
    }

    public void Set()
    {
        lock (_lock)
        {
            completitionSource.TrySetResult(null);
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            if (completitionSource.Task.IsCompleted) completitionSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}