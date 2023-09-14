namespace TokenBucketThrottler;

public interface ITokenBucketThrottler : IAsyncDisposable
{
    bool IsRefreshing { get; }

    ValueTask DisposeAsync();
    Task<bool> GetToken(CancellationToken cancellationToken = default);
    Task PauseRefresh(TimeSpan duration);
    Task ResumeRefresh();
    Task StopRefresh();
}
