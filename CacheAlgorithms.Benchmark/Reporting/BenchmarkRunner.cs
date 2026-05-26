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
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(workload);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(requestCount);

        // Materialise the request stream up front.
        // This isolates the timed section from any per-call generator overhead
        // (random number generation, binary search in the Zipfian CDF, etc.).
        var requests = workload.Generate(requestCount).ToArray();

        var evictionCount = 0;
        cache.Evicted += (_, _) => evictionCount++;

        // Warm up the JIT and CPU caches with a tiny dummy pass.
        // Without this, the first few hundred requests run slower than the rest
        // and skew the average. The warm-up volume is intentionally small —
        // we don't want to alter the cache state in a meaningful way.
        WarmUp(cache, workload.KeySpaceSize);
        cache.Clear();

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
                // The value is identical to the key here — we benchmark cache mechanics,
                // not what the cache stores. Using key-as-value avoids allocating
                // strings or objects inside the timed loop.
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
            RequestCount = requestCount,
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