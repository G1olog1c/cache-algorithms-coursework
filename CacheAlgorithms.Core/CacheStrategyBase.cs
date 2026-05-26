using System.Diagnostics.CodeAnalysis;

namespace CacheAlgorithms.Core;

/// <summary>
/// Provides a base implementation for cache eviction strategies.
/// Handles capacity validation and exposes a helper for raising the
/// <see cref="Evicted"/> event so derived classes can focus on the
/// strategy-specific algorithm.
/// </summary>
/// <typeparam name="TKey">The type of keys used to identify cached values.</typeparam>
/// <typeparam name="TValue">The type of values stored in the cache.</typeparam>
public abstract class CacheStrategyBase<TKey, TValue> : ICacheStrategy<TKey, TValue>
    where TKey : notnull
{
    /// <summary>
    /// Initializes a new instance of a cache strategy with the given capacity.
    /// </summary>
    /// <param name="capacity">The maximum number of entries the cache can hold.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="capacity"/> is less than or equal to zero.
    /// </exception>
    protected CacheStrategyBase(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        Capacity = capacity;
    }

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public int Capacity { get; }

    /// <inheritdoc />
    public abstract int Count { get; }

    /// <inheritdoc />
    public event EventHandler<CacheEvictionEventArgs<TKey>>? Evicted;

    /// <inheritdoc />
    public abstract bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue value);

    /// <inheritdoc />
    public abstract void Put(TKey key, TValue value);

    /// <inheritdoc />
    public abstract bool Contains(TKey key);

    /// <inheritdoc />
    public abstract bool Remove(TKey key);

    /// <inheritdoc />
    public abstract void Clear();

    /// <summary>
    /// Raises the <see cref="Evicted"/> event for the given key.
    /// Intended to be called by derived classes whenever they evict an entry.
    /// </summary>
    /// <param name="evictedKey">The key that was evicted.</param>
    protected void OnEvicted(TKey evictedKey)
    {
        Evicted?.Invoke(this, new CacheEvictionEventArgs<TKey>(evictedKey));
    }
}