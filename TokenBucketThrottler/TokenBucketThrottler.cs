using System.Net.Sockets;

namespace TokenBucketThrottler;

public class TokenBucketThrottler : ITokenBucketThrottler
{
    readonly CancellationTokenSource _cts;
    readonly AsyncLock _lock = new();
    readonly ITokenBucket _bucket;
    readonly IRefresher _refresher;

    Task _nextRefresh;
    Task _refreshTask;

    public TokenBucketThrottler(
        ITokenBucket bucket,
        TimeSpan refreshInterval,
        CancellationToken cancellationToken = default)
    : this(bucket, new DefaultRefresher(refreshInterval), cancellationToken)
    { }

    public TokenBucketThrottler(
        ITokenBucket bucket,
        IRefresher refresher,
        CancellationToken cancellationToken = default)
    {
        _bucket = bucket;
        _refresher = refresher;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _nextRefresh = refresher.Delay(_cts.Token);
        _refreshTask = Refresh(_cts.Token);
    }

    public bool IsRefreshing => _refreshTask.IsCompleted;

    private async Task Refresh(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _nextRefresh;
                _nextRefresh = _refresher.Delay(cancellationToken);
                await _bucket.Fill(1, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    public Task<bool> GetToken(CancellationToken cancellationToken = default)
        => _bucket.Retrieve(cancellationToken);

    public async Task ResumeRefresh()
    {
        if (IsRefreshing) { return; }

        using (await _lock.Lock())
        {
            _refreshTask = Refresh(_cts.Token);
        }
    }

    public async Task StopRefresh()
    {
        using var _ = await _lock.Lock();

        try
        {
            _cts.Cancel();
        }
        catch (AggregateException ex)
        {
            var t = ex.InnerException;
        }
        await _refreshTask;
    }

    public async Task PauseRefresh(TimeSpan duration)
    {
        _cts.Cancel();
        await Task.Delay(duration, _cts.Token);
        await _refreshTask.ContinueWith(async _ =>
        {
            await ResumeRefresh();
        });
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await StopRefresh();
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }
}

