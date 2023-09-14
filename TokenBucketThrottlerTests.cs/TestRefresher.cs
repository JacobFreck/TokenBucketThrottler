namespace TokenBucketThrottlerTests.cs;

using System;
using System.Threading;
using TokenBucketThrottler;

public class TestRefresher : IRefresher
{
    readonly AsyncGate _gate = new();

    public Task Delay(CancellationToken cancellationToken = default)
        => _gate.WaitForOpen(cancellationToken).ContinueWith(task => _gate.Reset(), cancellationToken);

    public Task TriggerRefresh(CancellationToken cancellationToken = default)
        => _gate.OpenAndClose(cancellationToken);

    public Task WaitForRefreshStarted(CancellationToken cancellationToken = default)
        => _gate.WaitForOpen(cancellationToken);
}
