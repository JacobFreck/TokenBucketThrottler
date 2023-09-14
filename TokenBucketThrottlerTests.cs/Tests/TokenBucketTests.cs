using System;
using TokenBucketThrottler;

namespace TokenBucketThrottlerTests;

public class TokenBucketTests
{
	[Fact]
	public async Task FillSucceedsOnUnfilledBucket()
	{
		TokenBucket b = new(maxCount: 1, startCount: 0);
		await b.Fill(1);
		Assert.Equal(1, 1);
	}

	[Theory]
    [InlineData(1, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 2)]
    public async Task RetrieveSucceedsOnNonEmptybucket(int startCount, int maxCount)
	{
        TokenBucket b = new(maxCount: maxCount, startCount: startCount);
        bool result = await b.Retrieve();
		Assert.True(result);
    }

	[Theory]
    [InlineData(0, 1)]
    [InlineData(0, 2)]
    [InlineData(0, 3)]
    public async Task RetrieveFailsOnEmptybucket(int startCount, int maxCount)
    {
        TokenBucket b = new(maxCount: maxCount, startCount: startCount);
        bool result = await b.Retrieve();
        Assert.False(result);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(0, 2)]
    [InlineData(1, 3)]
    [InlineData(0, 100)]
    [InlineData(3, 300)]
    public async Task MultiFillSuccedsUpToMaxCount(int startCount, int maxCount)
    {
        TokenBucket b = new(startCount: startCount, maxCount: maxCount);
        await Task.WhenAll(
            Enumerable.Range(startCount, maxCount).Select(async _ => await b.Fill(1)));
    }


    [Theory]
    [InlineData(0, 1)]
    [InlineData(0, 2)]
    [InlineData(1, 3)]
    [InlineData(0, 100)]
    [InlineData(3, 300)]
    public async Task MultiRetrieveSucceedsUntilEmpty(int startCount, int maxCount)
    {
        TokenBucket b = new(startCount: startCount, maxCount: maxCount);
        bool[] results = await Task.WhenAll(
            Enumerable.Range(0, startCount).Select(async _ => await b.Retrieve()));

        Assert.All(results, b => Assert.True(b));
    }
}

