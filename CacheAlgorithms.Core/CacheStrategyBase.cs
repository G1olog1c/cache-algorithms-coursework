using System.Diagnostics.CodeAnalysis;

namespace CacheAlgorithms.Core;

/// <summary>
/// Base class for cache eviction strategies. Handles capacity validation
/// and exposes <see cref="OnEvicted"/> so derived classes can focus on
/// the strategy-specific algorithm.
/// </summary>
public abstract class CacheStrategyBase<TKey, TValue> : ICacheStrategy<TKey, TValue>
    where TKey : notnull
{
    protected CacheStrategyBase(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        Capacity = capacity;
    }

    public abstract string Name { get; }

    public int Capacity { get; }

    public abstract int Count { get; }

    public event EventHandler<CacheEvictionEventArgs<TKey>>? Evicted;

    public abstract bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue value);

    public abstract void Put(TKey key, TValue value);

    public abstract bool Contains(TKey key);

    public abstract bool Remove(TKey key);

    public abstract void Clear();

    /// <summary>
    /// Raises <see cref="Evicted"/>. Called by derived classes whenever they
    /// remove an entry as part of the eviction policy (not via <see cref="Remove"/>).
    /// </summary>
    protected void OnEvicted(TKey evictedKey)
        => Evicted?.Invoke(this, new CacheEvictionEventArgs<TKey>(evictedKey));
}