namespace TokenBucketThrottler;

class DefaultRefresher : IRefresher
{
    TimeSpan _refreshInterval;

    public DefaultRefresher(TimeSpan refreshInterval)
        => _refreshInterval = refreshInterval;

    public Task Delay(CancellationToken cancellationToken)
        => Task.Delay(_refreshInterval, cancellationToken);
}

