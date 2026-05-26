using System.Diagnostics.CodeAnalysis;

namespace CacheAlgorithms.Core;

/// <summary>
/// Fixed-capacity cache with a pluggable eviction strategy.
/// Implementations decide which entry is discarded when a new one
/// must be inserted into a full cache.
/// </summary>
public interface ICacheStrategy<TKey, TValue>
    where TKey : notnull
{
    /// <summary>Human-readable name of the strategy (e.g. "LRU", "FIFO").</summary>
    string Name { get; }

    int Capacity { get; }

    int Count { get; }

    event EventHandler<CacheEvictionEventArgs<TKey>>? Evicted;

    /// <summary>
    /// Attempts to retrieve the value for <paramref name="key"/>.
    /// A successful lookup may update internal state used by the eviction policy
    /// (for example, recency or frequency counters).
    /// </summary>
    bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue value);

    /// <summary>
    /// Inserts a new entry or updates an existing one. When the cache is full
    /// and the key is new, exactly one entry is evicted according to the strategy
    /// and the <see cref="Evicted"/> event is raised.
    /// </summary>
    void Put(TKey key, TValue value);

    /// <summary>
    /// Checks whether the key is present without affecting the eviction policy.
    /// </summary>
    bool Contains(TKey key);

    bool Remove(TKey key);

    void Clear();
}