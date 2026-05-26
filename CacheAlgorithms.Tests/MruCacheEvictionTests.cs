using CacheAlgorithms.Core;

namespace CacheAlgorithms.Tests;

public class MruCacheEvictionTests
{
    [Fact]
    public void Evicts_most_recently_used_key()
    {
        var cache = new MruCache<int, string>(3);
        cache.Put(1, "a");
        cache.Put(2, "b");
        cache.Put(3, "c");

        cache.TryGet(1, out _); // 1 is now most recently used

        cache.Put(4, "d");

        // 1 was most recently used right before Put(4) — it goes out.
        Assert.False(cache.Contains(1));
        Assert.True(cache.Contains(2));
        Assert.True(cache.Contains(3));
        Assert.True(cache.Contains(4));
    }

    [Fact]
    public void Without_reads_behaves_like_a_stack()
    {
        // Without any TryGet calls, the most recently inserted key
        // is always the eviction target.
        var cache = new MruCache<int, string>(2);
        cache.Put(1, "a");
        cache.Put(2, "b");

        var evicted = new List<int>();
        cache.Evicted += (_, args) => evicted.Add(args.EvictedKey);

        cache.Put(3, "c"); // evicts 2 (most recent)
        cache.Put(4, "d"); // evicts 3 (most recent)

        Assert.Equal([2, 3], evicted);
        Assert.True(cache.Contains(1));
        Assert.True(cache.Contains(4));
    }

    [Fact]
    public void Beats_lru_on_cyclic_scan_pattern()
    {
        // Classic case: scanning a set larger than the cache, repeatedly.
        // LRU degrades to 0% hit rate; MRU keeps the early portion warm.
        const int cacheSize = 3;
        const int workingSetSize = 5;
        const int cycles = 3;

        var mru = new MruCache<int, int>(cacheSize);
        var hits = 0;
        var total = 0;

        for (var cycle = 0; cycle < cycles; cycle++)
        {
            for (var key = 0; key < workingSetSize; key++)
            {
                total++;
                if (mru.TryGet(key, out _))
                {
                    hits++;
                }
                else
                {
                    mru.Put(key, key);
                }
            }
        }

        // MRU should achieve a non-trivial hit rate here.
        // Exact number depends on implementation details, but it must be > 0,
        // which LRU would NOT achieve on this pattern.
        Assert.True(hits > 0,
            $"MRU produced {hits}/{total} hits on cyclic scan — expected > 0.");
    }
}