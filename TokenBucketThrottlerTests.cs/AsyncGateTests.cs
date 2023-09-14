using System;
namespace TokenBucketThrottlerTests.cs;

public class AsyncGateTests
{
	public AsyncGateTests() { }

    [Fact]
    public void GateStartsClosed()
    {
        AsyncGate gate = new();
        Assert.False(gate.IsOpen);
    }

    [Fact]
    public async Task GateIsClosedAfterOpenAndThenClosed()
    {
        AsyncGate gate = new();

        await gate.Open();
        await gate.Close();

        Assert.False(gate.IsOpen);
    }


    [Fact]
    public async Task GateIsClosedAfterOpenAndClose()
    {
        AsyncGate gate = new();

        await gate.OpenAndClose();

        Assert.False(gate.IsOpen);
    }

    [Fact]
    public async Task GateOpens()
    {
        AsyncGate gate = new();

        await gate.Open();

        Assert.True(gate.IsOpen);
    }


    [Fact]
    public async Task GateIsOpenAfterReopening()
    {
        AsyncGate gate = new();

        await gate.OpenAndClose();
        await gate.Open();

        Assert.True(gate.IsOpen);
    }


    [Fact]
    public async Task GateOpenAndClose()
    {
        AsyncGate gate = new();
        await gate.OpenAndClose();
        Assert.False(gate.IsOpen);
    }


    [Fact]
    public async Task WaitForOpenDoesNotCompleteWhenGateClosed()
    {
        AsyncGate gate = new();
        Task waitForOpen = gate.WaitForOpen();

        Task completedTask = await Task.WhenAny(
            Task.Delay(TimeSpan.FromMilliseconds(30)),
            waitForOpen);

        Assert.NotEqual(completedTask, waitForOpen);
    }

    [Fact]
    public async Task WaitForOpenCompletesWhenGateOpened()
    {
        AsyncGate gate = new();
        Task waitForOpen = gate.WaitForOpen();

        await gate.Open();

        CancellationTokenSource cts = new(delay: TimeSpan.FromMilliseconds(30));
        await gate.WaitForOpen(cts.Token);
    }

    [Fact]
    public async Task WaitForOpenCancelsAfterTimeout()
    {
        AsyncGate gate = new();
        Task waitForOpen = gate.WaitForOpen();

        CancellationTokenSource cts = new(delay: TimeSpan.FromMilliseconds(30));

        await Assert.ThrowsAsync<TaskCanceledException>(() => gate.WaitForOpen(cts.Token));
    }


    [Fact]
    public async Task WaitForCloseDoesNotCompleteWhenGateOpened()
    {
        AsyncGate gate = new();
        Task waitForOpen = gate.WaitForClose();

        await gate.Open();

        CancellationTokenSource cts = new(delay: TimeSpan.FromMilliseconds(30));
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => gate.WaitForClose(cts.Token));
    }


    [Fact]
    public async Task WaitForCloseDoesNotCompleteAfterOpenAndThenClose()
    {
        AsyncGate gate = new();

        await gate.Open();
        await gate.Close();

        CancellationTokenSource cts = new(delay: TimeSpan.FromMilliseconds(30));
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => gate.WaitForClose(cts.Token));
    }

    [Fact]
    public async Task WaitForCloseDoesNotCompleteAfterOpenAndClose()
    {
        AsyncGate gate = new();

        await gate.OpenAndClose();

        CancellationTokenSource cts = new(delay: TimeSpan.FromMilliseconds(30));
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => gate.WaitForClose(cts.Token));
    }

    [Fact]
    public async Task OpenOpenedGateThrowsInvalidOperationException()
    {
        AsyncGate gate = new();

        await gate.Open();

        await Assert.ThrowsAsync<InvalidOperationException>(() => gate.Open());
    }

    [Fact]
    public async Task CloseOnDefaultGateThrowsInvalidOperationException()
    {
        AsyncGate gate = new();

        await Assert.ThrowsAsync<InvalidOperationException>(() => gate.Close());
    }
}
