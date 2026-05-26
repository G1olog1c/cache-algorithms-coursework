using CacheAlgorithms.Core;

namespace CacheAlgorithms.Tests;

public class LfuCacheEvictionTests
{
    [Fact]
    public void Evicts_least_frequently_used_key()
    {
        var cache = new LfuCache<int, string>(3);
        cache.Put(1, "a");
        cache.Put(2, "b");
        cache.Put(3, "c");

        // Bump frequencies: key 1 → 3 total accesses, key 2 → 2, key 3 → 1.
        cache.TryGet(1, out _);
        cache.TryGet(1, out _);
        cache.TryGet(2, out _);

        cache.Put(4, "d");

        // Key 3 had the lowest frequency (just the initial Put).
        Assert.False(cache.Contains(3));
        Assert.True(cache.Contains(1));
        Assert.True(cache.Contains(2));
        Assert.True(cache.Contains(4));
    }

    [Fact]
    public void New_entries_start_with_lowest_frequency()
    {
        var cache = new LfuCache<int, string>(3);
        cache.Put(1, "a");
        cache.Put(2, "b");
        cache.Put(3, "c");

        // Bump every existing key above frequency 1 so the cache is "warm".
        cache.TryGet(1, out _);
        cache.TryGet(2, out _);
        cache.TryGet(3, out _);

        // Put(4) evicts one of {1, 2, 3} (they share freq=2; LRU tie-break picks 1).
        cache.Put(4, "d");
        Assert.False(cache.Contains(1));
        Assert.True(cache.Contains(4));

        // Now 4 has freq=1 while 2 and 3 have freq=2.
        // Put(5) must evict 4 — proving new entries truly start at the lowest frequency.
        cache.Put(5, "e");
        Assert.False(cache.Contains(4));
        Assert.True(cache.Contains(2));
        Assert.True(cache.Contains(3));
        Assert.True(cache.Contains(5));
    }

    [Fact]
    public void Update_counts_as_a_frequency_bump()
    {
        var cache = new LfuCache<int, string>(2);
        cache.Put(1, "a");
        cache.Put(2, "b");

        cache.Put(1, "a-updated"); // frequency of 1 becomes 2; 2 stays at 1

        cache.Put(3, "c");

        // Key 2 was the least frequently used.
        Assert.False(cache.Contains(2));
        Assert.True(cache.Contains(1));
        Assert.True(cache.Contains(3));
    }

    [Fact]
    public void Tie_break_uses_lru_within_same_frequency()
    {
        // All three keys end with frequency 1. The one accessed earliest must go.
        var cache = new LfuCache<int, string>(3);
        cache.Put(1, "a"); // added first
        cache.Put(2, "b");
        cache.Put(3, "c"); // added last

        int? evictedKey = null;
        cache.Evicted += (_, args) => evictedKey = args.EvictedKey;

        cache.Put(4, "d");

        Assert.Equal(1, evictedKey);
    }
}