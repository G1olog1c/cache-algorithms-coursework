using System.Diagnostics;
using CacheAlgorithms.Core;
using CacheAlgorithms.Benchmark.Workload;

namespace CacheAlgorithms.Benchmark.Reporting;

/// <summary>
/// Executes a single benchmark: feeds a generated request stream through a cache
/// strategy, measures hit/miss counts and elapsed time, and returns the result.
/// </summary>
public static class BenchmarkRunner
{
    /// <summary>
    /// Runs one experiment. The request stream is fully materialised before timing
    /// starts so that workload generation cost is excluded from the measurement.
    /// </summary>
    public static BenchmarkResult Run(
        ICacheStrategy<int, int> cache,
        IWorkloadGenerator workload,
        int requestCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(requestCount);
        var requests = workload.Generate(requestCount).ToArray();
        return Run(cache, workload, requests);
    }

    public static BenchmarkResult Run(
        ICacheStrategy<int, int> cache,
        IWorkloadGenerator workload,
        IReadOnlyList<int> requests)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(workload);
        ArgumentNullException.ThrowIfNull(requests);

        WarmUp(cache, workload.KeySpaceSize);
        cache.Clear();

        var evictionCount = 0;
        cache.Evicted += (_, _) => evictionCount++;

        var hits = 0;
        var misses = 0;

        var stopwatch = Stopwatch.StartNew();
        foreach (var key in requests)
        {
            if (cache.TryGet(key, out _))
            {
                hits++;
            }
            else
            {
                misses++;
                cache.Put(key, key);
            }
        }
        stopwatch.Stop();

        return new BenchmarkResult
        {
            StrategyName = cache.Name,
            WorkloadName = workload.Name,
            Capacity = cache.Capacity,
            KeySpaceSize = workload.KeySpaceSize,
            RequestCount = requests.Count,
            Hits = hits,
            Misses = misses,
            Evictions = evictionCount,
            TotalDuration = stopwatch.Elapsed,
        };
    }

    private static void WarmUp(ICacheStrategy<int, int> cache, int keySpaceSize)
    {
        const int warmUpRequests = 100;
        for (var i = 0; i < warmUpRequests; i++)
        {
            var key = i % keySpaceSize;
            if (!cache.TryGet(key, out _))
            {
                cache.Put(key, key);
            }
        }
    }
}