using System;
namespace TokenBucketThrottlerTests.cs;

public class TestRefresherTests
{
    [Fact]
    public async Task TestRefresherTriggerRefreshAllowDelayToProceed()
    {
        TestRefresher refresher = new();

        CancellationTokenSource cts = new(delay: TimeSpan.FromMilliseconds(30));

        await Assert.ThrowsAsync<TaskCanceledException>(() => refresher.Delay(cts.Token));
    }

    [Fact]
    public async Task TestRefresherRefreshAllowsDelayToComplete()
    {
        TestRefresher refresher = new();

        CancellationTokenSource cts = new(delay: TimeSpan.FromMilliseconds(30));
        Task delayTask = refresher.Delay(cts.Token);
        await refresher.TriggerRefresh();

        await delayTask;
    }

    [Fact]
    public async Task TestRefresherRefreshAllowsDelayToCompleteDoubleAwait()
    {
        TestRefresher refresher = new();

        CancellationTokenSource cts = new(delay: TimeSpan.FromMilliseconds(30));
        Task delayTask = refresher.Delay(cts.Token);
        await refresher.TriggerRefresh();

        await delayTask;
        await delayTask;
    }


    [Fact]
    public async Task RefreshAllowsDelayToCompleteNextDelayDoesNotProceed()
    {
        TestRefresher refresher = new();

        CancellationTokenSource cts = new(delay: TimeSpan.FromMilliseconds(50));
        Task delayTask = refresher.Delay(cts.Token);
        await refresher.TriggerRefresh();

        await delayTask;
        await Assert.ThrowsAsync<TaskCanceledException>(() => refresher.Delay(cts.Token));
    }
}

