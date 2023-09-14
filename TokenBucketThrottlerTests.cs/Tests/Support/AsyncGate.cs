namespace TokenBucketThrottlerTests;

using System.Threading;
using TokenBucketThrottler;

public class AsyncGate
{
    readonly AsyncLock _lock = new();
    TaskCompletionSource _openCompletionSource = new();
    TaskCompletionSource _closeCompletionSource = new();

    public bool IsOpen { get; private set; }

    /// <summary>
    /// Wait for the gate to be opened.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>Task that completes whent the gate has opened</returns>
    public async Task WaitForOpen(CancellationToken cancellationToken = default)
    {
        using (cancellationToken.Register(_openCompletionSource.SetCanceled))
            await _openCompletionSource.Task;
    }

    /// <summary>
    /// Wait for the gate to be closed.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task WaitForClose(CancellationToken cancellationToken = default)
    {
        using (cancellationToken.Register(_closeCompletionSource.SetCanceled))
            await _closeCompletionSource.Task;
    }

    /// <summary>
    /// Opens the AsyncGate
    /// </summary>
    /// <throws>InvalidOperationException</throws>
    /// <returns></returns>
    public async Task Open(CancellationToken cancellationToken = default)
    {
        using (await _lock.Lock(cancellationToken))
        {
            if (IsOpen)
                throw new InvalidOperationException("AsyncGate already opened.");

            UnlockedOpen();
        }
    }

    private void UnlockedOpen()
    {
        _openCompletionSource.SetResult();
        IsOpen = true;
    }

    /// <summary>
    /// Closes the AsyncGate. After closed, the gate is available to be reopened
    /// </summary>
    /// <returns></returns>
    public async Task Close(CancellationToken cancellationToken = default)
    {
        using (await _lock.Lock(cancellationToken))
        {
            if (!IsOpen)
                throw new InvalidOperationException("AsyncGate is already closed.");

            UnlockedClose();
        }
    }

    private void UnlockedClose()
    {
        IsOpen = false;
        _closeCompletionSource.SetResult();
        Reset();
    }


    public async Task OpenAndClose(CancellationToken cancellationToken = default)
    {
        using (await _lock.Lock(cancellationToken))
        {
            if (IsOpen)
                throw new InvalidOperationException("AsyncGate already opened.");

            UnlockedOpen();
            UnlockedClose();
        }
    }

    public void Reset()
    {
        _openCompletionSource = new TaskCompletionSource();
        _closeCompletionSource = new TaskCompletionSource();
    }
}
