namespace CacheAlgorithms;

/// <summary>
/// Event arguments raised when a cache strategy evicts an entry.
/// </summary>
/// <typeparam name="TKey">The type of the evicted key.</typeparam>
public sealed class CacheEvictionEventArgs<TKey> : EventArgs
    where TKey : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CacheEvictionEventArgs{TKey}"/> class.
    /// </summary>
    /// <param name="evictedKey">The key that was evicted from the cache.</param>

    public CacheEvictionEventArgs(TKey evictedKey)
    {
        EvictedKey = evictedKey;
    }
    
    /// <summary>
    /// Gets the key that was evicted from the cache.
    /// </summary>
    public TKey EvictedKey { get; }
}