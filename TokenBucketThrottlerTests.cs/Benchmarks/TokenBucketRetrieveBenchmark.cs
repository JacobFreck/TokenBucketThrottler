using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using TokenBucketThrottler;
using Xunit.Abstractions;

namespace TokenBucketThrottlerBenchmarks;


[MemoryDiagnoser]
public class TokenBucketRetrieveBenchmark
{
    [Benchmark]
    public async Task RetrieveBenchmark()
    {
        TokenBucket b = new(maxCount: 1);
        await b.Retrieve();
    }
}


public class TokenBucketRetrieveBenchmarkTests
{
    readonly ITestOutputHelper _output;
    readonly BenchmarkReport _report;
    readonly Statistics _stats;
    readonly GcStats _gcStats;

    public TokenBucketRetrieveBenchmarkTests(ITestOutputHelper output)
    {
        _output = output;
        var logger = new AccumulationLogger();

        var config = ManualConfig.Create(DefaultConfig.Instance)
            .AddLogger(logger)
            .WithOptions(ConfigOptions.DisableOptimizationsValidator);

        Summary summary = BenchmarkRunner.Run<TokenBucketRetrieveBenchmark>(config);

        // write benchmark summary
        _output.WriteLine(logger.GetLog());

        BenchmarkReport report = summary.Reports.First();
        _report = report;
        _stats = report.ResultStatistics!;
        _gcStats = report.GcStats;
    }

    [Fact]
    public void TokenBucketRetrieveBenchmark_MeetsPerformanceTarget()
    {
        _output.WriteLine($"P95 is {_stats.Percentiles.P95}");
        Assert.True(100 >= _stats.Percentiles.P95, $"P95 is {_stats.Percentiles.P95}");
    }

    [Fact]
    public void TokenBucketRetrieveBenchmark_MeetsMemoryAllocationLimit()
    {
        _output.WriteLine($"P95 is {_stats.Percentiles.P95}");
        BenchmarkCase retrieveCase = _report.BenchmarkCase;
        Assert.True(250 >= _gcStats.GetBytesAllocatedPerOperation(retrieveCase),
            $"Bytes allocated per operation {_gcStats.GetBytesAllocatedPerOperation(retrieveCase)}");
    }
}
