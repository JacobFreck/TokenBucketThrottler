namespace TokenBucketThrottler;

public class TokenBucket : ITokenBucket
{
    int _count;
    readonly int _maxCount;
    readonly AsyncLock _lock = new();

    public int MaxCount => _maxCount;

    public TokenBucket(int maxCount, int? startCount = null)
    {
        _maxCount = maxCount;
        _count = startCount ?? maxCount;
    }

    public virtual async Task Fill(int count, CancellationToken cancellationToken = default)
    {
        using (await _lock.Lock(cancellationToken))
        {
            _count = Math.Min(_count + count, MaxCount);
        }
    }

    public virtual async Task<bool> Retrieve(CancellationToken cancellationToken = default)
    {
        using (await _lock.Lock(cancellationToken))
        {
            if (_count > 0)
            {
                _count--;
                return true;
            }

            return false;
        }
    }
}

