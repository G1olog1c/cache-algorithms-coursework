using CacheAlgorithms.Core;

namespace CacheAlgorithms.Tests;

/// <summary>
/// Contract tests run against every ICacheStrategy implementation.
/// Verify behaviour that should hold regardless of the eviction policy:
/// basic storage semantics, capacity enforcement, removal, events.
/// </summary>
public class CommonContractTests
{
    // xUnit's [Theory] + [MemberData] feeds the same test method
    // multiple times, once per strategy. Each strategy is supplied
    // as a factory so tests get a fresh instance every run.
    public static IEnumerable<object[]> Strategies =>
    [
        [(Func<int, ICacheStrategy<int, string>>)(c => new FifoCache<int, string>(c))],
        [(Func<int, ICacheStrategy<int, string>>)(c => new LruCache<int, string>(c))],
        [(Func<int, ICacheStrategy<int, string>>)(c => new LfuCache<int, string>(c))],
        [(Func<int, ICacheStrategy<int, string>>)(c => new MruCache<int, string>(c))],
    ];

    [Theory]
    [MemberData(nameof(Strategies))]
    public void New_cache_is_empty(Func<int, ICacheStrategy<int, string>> factory)
    {
        var cache = factory(3);

        Assert.Equal(0, cache.Count);
        Assert.Equal(3, cache.Capacity);
    }

    [Theory]
    [MemberData(nameof(Strategies))]
    public void Put_adds_entries_up_to_capacity(Func<int, ICacheStrategy<int, string>> factory)
    {
        var cache = factory(3);

        cache.Put(1, "a");
        cache.Put(2, "b");
        cache.Put(3, "c");

        Assert.Equal(3, cache.Count);
        Assert.True(cache.Contains(1));
        Assert.True(cache.Contains(2));
        Assert.True(cache.Contains(3));
    }

    [Theory]
    [MemberData(nameof(Strategies))]
    public void TryGet_returns_stored_value_on_hit(Func<int, ICacheStrategy<int, string>> factory)
    {
        var cache = factory(3);
        cache.Put(42, "answer");

        var found = cache.TryGet(42, out var value);

        Assert.True(found);
        Assert.Equal("answer", value);
    }

    [Theory]
    [MemberData(nameof(Strategies))]
    public void TryGet_returns_false_on_miss(Func<int, ICacheStrategy<int, string>> factory)
    {
        var cache = factory(3);
        cache.Put(1, "a");

        var found = cache.TryGet(999, out var value);

        Assert.False(found);
        Assert.Null(value);
    }

    [Theory]
    [MemberData(nameof(Strategies))]
    public void Put_with_existing_key_updates_value(Func<int, ICacheStrategy<int, string>> factory)
    {
        var cache = factory(3);
        cache.Put(1, "old");
        cache.Put(1, "new");

        Assert.Equal(1, cache.Count);
        cache.TryGet(1, out var value);
        Assert.Equal("new", value);
    }

    [Theory]
    [MemberData(nameof(Strategies))]
    public void Count_never_exceeds_capacity(Func<int, ICacheStrategy<int, string>> factory)
    {
        var cache = factory(3);

        for (var i = 0; i < 100; i++)
        {
            cache.Put(i, $"v{i}");
            Assert.True(cache.Count <= 3,
                $"Count={cache.Count} exceeded Capacity=3 after Put({i})");
        }
    }

    [Theory]
    [MemberData(nameof(Strategies))]
    public void Remove_takes_entry_out_of_cache(Func<int, ICacheStrategy<int, string>> factory)
    {
        var cache = factory(3);
        cache.Put(1, "a");
        cache.Put(2, "b");

        var removed = cache.Remove(1);

        Assert.True(removed);
        Assert.Equal(1, cache.Count);
        Assert.False(cache.Contains(1));
        Assert.True(cache.Contains(2));
    }

    [Theory]
    [MemberData(nameof(Strategies))]
    public void Remove_returns_false_for_unknown_key(Func<int, ICacheStrategy<int, string>> factory)
    {
        var cache = factory(3);
        cache.Put(1, "a");

        var removed = cache.Remove(999);

        Assert.False(removed);
        Assert.Equal(1, cache.Count);
    }

    [Theory]
    [MemberData(nameof(Strategies))]
    public void Clear_empties_the_cache(Func<int, ICacheStrategy<int, string>> factory)
    {
        var cache = factory(3);
        cache.Put(1, "a");
        cache.Put(2, "b");

        cache.Clear();

        Assert.Equal(0, cache.Count);
        Assert.False(cache.Contains(1));
        Assert.False(cache.Contains(2));
    }

    [Theory]
    [MemberData(nameof(Strategies))]
    public void Evicted_event_fires_exactly_once_per_eviction(Func<int, ICacheStrategy<int, string>> factory)
    {
        var cache = factory(2);
        var evictedKeys = new List<int>();
        cache.Evicted += (_, args) => evictedKeys.Add(args.EvictedKey);

        cache.Put(1, "a");
        cache.Put(2, "b");
        cache.Put(3, "c"); // capacity exceeded — should evict exactly one entry

        Assert.Single(evictedKeys);
    }

    [Theory]
    [MemberData(nameof(Strategies))]
    public void Evicted_event_does_not_fire_on_update(Func<int, ICacheStrategy<int, string>> factory)
    {
        var cache = factory(3);
        var evictionCount = 0;
        cache.Evicted += (_, _) => evictionCount++;

        cache.Put(1, "first");
        cache.Put(1, "second"); // overwrite of existing key, not eviction

        Assert.Equal(0, evictionCount);
    }

    [Theory]
    [MemberData(nameof(Strategies))]
    public void Constructor_rejects_zero_capacity(Func<int, ICacheStrategy<int, string>> factory)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => factory(0));
    }

    [Theory]
    [MemberData(nameof(Strategies))]
    public void Constructor_rejects_negative_capacity(Func<int, ICacheStrategy<int, string>> factory)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => factory(-1));
    }

    [Theory]
    [MemberData(nameof(Strategies))]
    public void Name_is_not_empty(Func<int, ICacheStrategy<int, string>> factory)
    {
        var cache = factory(1);
        Assert.False(string.IsNullOrWhiteSpace(cache.Name));
    }
}