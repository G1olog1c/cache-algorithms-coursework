namespace CacheAlgorithms.Benchmark.Reporting;

/// <summary>
/// Immutable record of a single benchmark run: one strategy × one workload × one capacity.
/// Carries everything needed to produce reports without re-running the experiment.
/// </summary>
public sealed record BenchmarkResult
{
    public required string StrategyName { get; init; }

    public required string WorkloadName { get; init; }

    public required int Capacity { get; init; }

    public required int KeySpaceSize { get; init; }

    public required int RequestCount { get; init; }

    public required int Hits { get; init; }

    public required int Misses { get; init; }

    public required int Evictions { get; init; }

    public required TimeSpan TotalDuration { get; init; }

    public double HitRate => RequestCount == 0 ? 0.0 : (double)Hits / RequestCount;

    public double MissRate => RequestCount == 0 ? 0.0 : (double)Misses / RequestCount;

    public double AverageNanosecondsPerRequest =>
        RequestCount == 0 ? 0.0 : TotalDuration.TotalNanoseconds / RequestCount;
}