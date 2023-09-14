namespace TokenBucketThrottler;

public interface IRefresher
{
    Task Delay(CancellationToken cancellationToken);
}

