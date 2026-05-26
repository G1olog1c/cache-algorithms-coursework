using CacheAlgorithms.Core;

namespace CacheAlgorithms.Tests;

public class FifoCacheEvictionTests
{
    [Fact]
    public void Evicts_oldest_inserted_key_regardless_of_access()
    {
        var cache = new FifoCache<int, string>(3);
        cache.Put(1, "a");
        cache.Put(2, "b");
        cache.Put(3, "c");

        // Access 1 — under FIFO this must NOT save it from eviction.
        cache.TryGet(1, out _);

        cache.Put(4, "d");

        // 1 was inserted first, so it goes out — even though we just read it.
        Assert.False(cache.Contains(1));
        Assert.True(cache.Contains(2));
        Assert.True(cache.Contains(3));
        Assert.True(cache.Contains(4));
    }

    [Fact]
    public void Update_does_not_reset_insertion_order()
    {
        var cache = new FifoCache<int, string>(2);
        cache.Put(1, "a");
        cache.Put(2, "b");
        cache.Put(1, "a-updated"); // overwrite must not move key 1 in the queue

        cache.Put(3, "c");

        // Key 1 was still inserted earliest — it goes out, not key 2.
        Assert.False(cache.Contains(1));
        Assert.True(cache.Contains(2));
        Assert.True(cache.Contains(3));
    }

    [Fact]
    public void Eviction_reports_correct_key()
    {
        var cache = new FifoCache<int, string>(2);
        int? evictedKey = null;
        cache.Evicted += (_, args) => evictedKey = args.EvictedKey;

        cache.Put(10, "a");
        cache.Put(20, "b");
        cache.Put(30, "c");

        Assert.Equal(10, evictedKey);
    }
}