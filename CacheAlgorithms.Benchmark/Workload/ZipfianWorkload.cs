namespace CacheAlgorithms.Benchmark.Workload;

/// <summary>
/// Generates requests following the Zipf distribution: the probability of
/// requesting the i-th most popular key is proportional to 1 / i^skew.
/// Models real-world workloads where a small subset of "hot" keys dominates
/// traffic (web caches, social media feeds, database hot pages).
/// </summary>
/// <remarks>
/// Higher <c>skew</c> values concentrate more traffic on fewer keys.
/// Typical real-world values fall between 0.8 (mild skew, e.g. file accesses)
/// and 1.2 (strong skew, e.g. social network "celebrities").
/// A skew of 1.0 corresponds to the classic Zipf's law observed in natural language.
/// </remarks>
public sealed class ZipfianWorkload : IWorkloadGenerator
{
    private readonly Random _random;
    private readonly double[] _cumulativeProbabilities;
    private readonly double _skew;

    public ZipfianWorkload(int keySpaceSize, double skew, int seed)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(keySpaceSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(skew);

        KeySpaceSize = keySpaceSize;
        _skew = skew;
        _random = new Random(seed);
        _cumulativeProbabilities = BuildCumulativeProbabilities(keySpaceSize, skew);
    }

    public string Name => $"Zipfian(s={_skew:F2})";

    public int KeySpaceSize { get; }

    public IEnumerable<int> Generate(int requestCount)
    {
        for (var i = 0; i < requestCount; i++)
        {
            yield return SampleZipfian();
        }
    }

    /// <summary>
    /// Precomputes the cumulative distribution function for the Zipf distribution.
    /// Doing it once at construction lets each sample run in O(log n) via binary search,
    /// rather than recomputing the CDF on every draw.
    /// </summary>
    private static double[] BuildCumulativeProbabilities(int n, double skew)
    {
        var cumulative = new double[n];

        // Harmonic number H_n,s = sum of 1 / i^s — the normalising constant.
        var harmonic = 0.0;
        for (var i = 1; i <= n; i++)
        {
            harmonic += 1.0 / Math.Pow(i, skew);
        }

        var running = 0.0;
        for (var i = 1; i <= n; i++)
        {
            running += 1.0 / Math.Pow(i, skew) / harmonic;
            cumulative[i - 1] = running;
        }

        // Ensure the last bucket is exactly 1.0 to compensate for floating-point drift.
        cumulative[n - 1] = 1.0;
        return cumulative;
    }

    private int SampleZipfian()
    {
        var roll = _random.NextDouble();

        // Binary search for the smallest index whose cumulative probability >= roll.
        // The returned rank (1..N) is then mapped to a key id by subtracting 1.
        var index = Array.BinarySearch(_cumulativeProbabilities, roll);
        if (index < 0)
        {
            index = ~index;
        }
        return Math.Min(index, KeySpaceSize - 1);
    }
}