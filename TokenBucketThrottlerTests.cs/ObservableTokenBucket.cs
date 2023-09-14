using System;
using TokenBucketThrottler;

namespace TokenBucketThrottlerTests.cs;

public class ObservableTokenBucket : ITokenBucket
{
    readonly TokenBucket _bucket;

    public event Action? FillCompleted;
    public event Action? RetrieveCompleted;

    public int MaxCount => _bucket.MaxCount;

    public ObservableTokenBucket(int maxCount, int? startCount = null)
    {
        _bucket = new TokenBucket(maxCount, startCount);
    }

    public async Task Fill(int count, CancellationToken cancellationToken = default)
    {
        await _bucket.Fill(count, cancellationToken);
        FillCompleted?.Invoke();
    }

    public async Task<bool> Retrieve(CancellationToken cancellationToken = default)
    {
        bool result = await _bucket.Retrieve();
        RetrieveCompleted?.Invoke();
        return result;
    }
}

