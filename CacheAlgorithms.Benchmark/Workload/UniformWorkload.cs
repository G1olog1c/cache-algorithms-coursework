namespace CacheAlgorithms.Benchmark.Workload;

/// <summary>
/// Generates requests where every key in the space has equal probability.
/// Represents the worst case for any caching strategy: with no temporal or
/// frequency-based pattern to exploit, hit rate is bounded by capacity / keySpaceSize.
/// </summary>
public sealed class UniformWorkload : IWorkloadGenerator
{
    private readonly Random _random;

    public UniformWorkload(int keySpaceSize, int seed)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(keySpaceSize);

        KeySpaceSize = keySpaceSize;
        _random = new Random(seed);
    }

    public string Name => "Uniform";

    public int KeySpaceSize { get; }

    public IEnumerable<int> Generate(int requestCount)
    {
        for (var i = 0; i < requestCount; i++)
        {
            yield return _random.Next(0, KeySpaceSize);
        }
    }
}