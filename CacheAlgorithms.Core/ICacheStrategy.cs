using System.Diagnostics.CodeAnalysis;

namespace CacheAlgorithms.Core;

/// <summary>
/// Defines the contract for a fixed-capacity cache with a pluggable eviction strategy.
/// Implementations decide which entry is discarded when a new entry must be inserted
/// into a full cache.
/// </summary>
/// <typeparam name="TKey">The type of keys used to identify cached values.</typeparam>
/// <typeparam name="TValue">The type of values stored in the cache.</typeparam>
public interface ICacheStrategy<TKey, TValue>
    where TKey : notnull
{
    /// <summary>
    /// Gets the human-readable name of the eviction strategy (for example, "LRU", "FIFO").
    /// Used primarily for reporting and benchmark output.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the maximum number of entries the cache can hold.
    /// </summary>
    int Capacity { get; }

    /// <summary>
    /// Gets the current number of entries stored in the cache.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Occurs when an entry is evicted from the cache to make room for a new one.
    /// </summary>
    event EventHandler<CacheEvictionEventArgs<TKey>>? Evicted;

    /// <summary>
    /// Attempts to retrieve the value associated with the specified key.
    /// A successful lookup is considered a cache hit and may update the
    /// internal state used by the eviction policy (for example, recency or frequency).
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <param name="value">
    /// When this method returns <c>true</c>, contains the value associated with the key;
    /// otherwise, the default value of <typeparamref name="TValue"/>.
    /// </param>
    /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
    bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue value);

    /// <summary>
    /// Inserts a new entry into the cache or updates the value of an existing entry.
    /// If the cache is at capacity and the key is new, exactly one entry is evicted
    /// according to the strategy and the <see cref="Evicted"/> event is raised.
    /// </summary>
    /// <param name="key">The key under which to store the value.</param>
    /// <param name="value">The value to store.</param>
    void Put(TKey key, TValue value);

    /// <summary>
    /// Determines whether the specified key is present in the cache without
    /// affecting the eviction policy's internal state.
    /// </summary>
    /// <param name="key">The key to test for presence.</param>
    /// <returns><c>true</c> if the key is in the cache; otherwise, <c>false</c>.</returns>
    bool Contains(TKey key);

    /// <summary>
    /// Removes the entry with the specified key from the cache, if it exists.
    /// </summary>
    /// <param name="key">The key of the entry to remove.</param>
    /// <returns><c>true</c> if an entry was removed; <c>false</c> if the key was not found.</returns>
    bool Remove(TKey key);

    /// <summary>
    /// Removes all entries from the cache.
    /// </summary>
    void Clear();
}