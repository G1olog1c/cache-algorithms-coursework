namespace CacheAlgorithms.Benchmark.Workload;

/// <summary>
/// Produces a stream of integer keys representing cache access requests.
/// Implementations differ in the probability distribution they use to draw keys
/// from the key space, which directly determines how cache strategies behave.
/// </summary>
public interface IWorkloadGenerator
{
    /// <summary>Short human-readable label used in reports (e.g. "Uniform", "Zipfian").</summary>
    string Name { get; }

    /// <summary>Total number of distinct keys the generator may produce.</summary>
    int KeySpaceSize { get; }

    /// <summary>
    /// Generates the next <paramref name="requestCount"/> keys.
    /// The same instance produces a deterministic sequence given its seed,
    /// which is essential for reproducible benchmark runs.
    /// </summary>
    IEnumerable<int> Generate(int requestCount);
}