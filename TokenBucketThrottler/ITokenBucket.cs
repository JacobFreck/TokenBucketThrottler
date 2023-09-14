namespace TokenBucketThrottler;

public interface ITokenBucket
{
    int MaxCount { get; }

    Task Fill(int count, CancellationToken cancellationToken = default);
    Task<bool> Retrieve(CancellationToken cancellationToken = default);
}