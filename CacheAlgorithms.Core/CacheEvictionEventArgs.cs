namespace CacheAlgorithms.Core;

/// <summary>
/// Event data for entries evicted by a cache strategy.
/// </summary>
public sealed class CacheEvictionEventArgs<TKey> : EventArgs
    where TKey : notnull
{
    public CacheEvictionEventArgs(TKey evictedKey)
    {
        EvictedKey = evictedKey;
    }

    public TKey EvictedKey { get; }
}