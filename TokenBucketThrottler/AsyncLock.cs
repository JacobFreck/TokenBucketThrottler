namespace TokenBucketThrottler;

public class AsyncLock
{
    readonly SemaphoreSlim _semaphore = new(1, 1);
    readonly Task<Releaser> _cachedReleaser;

    public AsyncLock()
    {
        _cachedReleaser = Task.FromResult(new Releaser(this));
    }

    public Task<Releaser> Lock(CancellationToken cancellationToken = default)
    {
        Task wait = _semaphore.WaitAsync(cancellationToken);
        return wait.IsCompleted
            ? _cachedReleaser
            : wait.ContinueWith(
                continuationFunction: (_, state) => new Releaser(state as AsyncLock),
                state: this,
                cancellationToken: cancellationToken,
                continuationOptions: TaskContinuationOptions.ExecuteSynchronously,
                scheduler: TaskScheduler.Default);
    }

    public readonly struct Releaser : IDisposable
    {
        readonly AsyncLock _lock;

        internal Releaser(AsyncLock toRelease)
            => _lock = toRelease;

        public void Dispose()
            => _lock?._semaphore.Release();
    }
}

