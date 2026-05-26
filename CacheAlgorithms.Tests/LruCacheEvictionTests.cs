using CacheAlgorithms.Core;

namespace CacheAlgorithms.Tests;

public class LruCacheEvictionTests
{
    [Fact]
    public void Evicts_least_recently_used_key()
    {
        var cache = new LruCache<int, string>(3);
        cache.Put(1, "a");
        cache.Put(2, "b");
        cache.Put(3, "c");

        cache.TryGet(1, out _); // access order is now: 1 (newest), 3, 2 (oldest)

        cache.Put(4, "d");

        // 2 was least recently used.
        Assert.False(cache.Contains(2));
        Assert.True(cache.Contains(1));
        Assert.True(cache.Contains(3));
        Assert.True(cache.Contains(4));
    }

    [Fact]
    public void Update_counts_as_a_use()
    {
        var cache = new LruCache<int, string>(3);
        cache.Put(1, "a");
        cache.Put(2, "b");
        cache.Put(3, "c");

        cache.Put(1, "a-updated"); // promotes 1 to most recent

        cache.Put(4, "d");

        // Now 2 is least recently used.
        Assert.False(cache.Contains(2));
        Assert.True(cache.Contains(1));
    }

    [Fact]
    public void Classic_textbook_scenario()
    {
        // Capacity 3, sequence: Put A, Put B, Put C, Get A, Put D
        // Expected: B is evicted (LRU order after Get A is A, C, B).
        var cache = new LruCache<char, int>(3);
        int? evictedKey = null;
        cache.Evicted += (_, args) => evictedKey = args.EvictedKey;

        cache.Put('A', 1);
        cache.Put('B', 2);
        cache.Put('C', 3);
        cache.TryGet('A', out _);
        cache.Put('D', 4);

        Assert.Equal('B', evictedKey);
    }
}