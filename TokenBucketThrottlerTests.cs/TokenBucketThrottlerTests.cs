namespace TokenBucketThrottlerTests.cs;
using TokenBucketThrottler;

public class TokenBucketThrottlerTests
{
    [Fact]
    public async Task CanRetrieveToken()
    {
        await using var throttler = new TokenBucketThrottler(
            bucket: new TokenBucket(maxCount: 1),
            refresher: new TestRefresher());

        Assert.True(await throttler.GetToken());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task ExpectedRetrieveTokenFailure(int count)
    {
        await using var throttler = new TokenBucketThrottler(
            bucket: new TokenBucket(maxCount: count),
            refresher: new TestRefresher());

        foreach (int _ in Enumerable.Range(0, count))
            Assert.True(await throttler.GetToken());

        Assert.False(await throttler.GetToken());
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 10)]
    [InlineData(3, 100)]
    public async Task RefreshFilledBucketLeaks(int maxCount, int refreshCount)
    {
        AsyncGate gate = new();
        TestRefresher refresher = new();
        ObservableTokenBucket bucket = new(maxCount: maxCount, startCount: 0);

        await using var throttler = new TokenBucketThrottler(
            bucket: bucket,
            refresher: new TestRefresher());

        foreach (int _ in Enumerable.Range(0, refreshCount))
        {
            await refresher.TriggerRefresh();            
        }
            

        Assert.Equal(maxCount, bucket.MaxCount);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task DisposeThrottler(int count)
    {
        foreach (int _ in Enumerable.Range(0, count))
        {
            TokenBucket bucket = new(maxCount: 1);
            TestRefresher refresher = new();
            await using var throttler = new TokenBucketThrottler(
                bucket: bucket,
                refresher: refresher);
        }
    }

    [Fact]
    public async Task GetTokenSucceeds()
    {
        ObservableTokenBucket bucket = new(maxCount: 1);
        TaskCompletionSource tcs = new();
        int count = 0;
        bucket.FillCompleted += () =>
        {
            count++;
        };

        TestRefresher refresher = new();
        await using var throttler = new TokenBucketThrottler(
            bucket: bucket,
            refresher: refresher);

        bool result = await throttler.GetToken();
        Assert.True(result);
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task CountIncreasesOnRefresh()
    {
        ObservableTokenBucket bucket = new(maxCount: 1);
        TaskCompletionSource tcs = new();
        int count = 0;
        bucket.FillCompleted += () =>
        {
            count++;
            tcs.SetResult();
        };

        TestRefresher refresher = new();
        await using var throttler = new TokenBucketThrottler(
            bucket: bucket,
            refresher: refresher);


        await refresher.TriggerRefresh();
        await tcs.Task;
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task ConcurrentGetTokenSucceeds()
    {
    }
}
