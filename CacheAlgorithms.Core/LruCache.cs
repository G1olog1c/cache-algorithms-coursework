using System.Diagnostics.CodeAnalysis;

namespace CacheAlgorithms.Core;

/// <summary>
/// Fixed-capacity cache with Least Recently Used eviction policy.
/// On every access (read or write) the entry becomes the most recently used.
/// When the cache is full, the entry that has not been accessed for the longest
/// time is evicted. All operations run in O(1) time.
/// </summary>
public sealed class LruCache<TKey, TValue> : CacheStrategyBase<TKey, TValue>
    where TKey : notnull
{
    // The node stores both key and value: we need the key when evicting from the
    // tail so we can also remove it from the lookup dictionary.
    private readonly LinkedList<CacheEntry> _accessOrder;
    private readonly Dictionary<TKey, LinkedListNode<CacheEntry>> _lookup;

    public LruCache(int capacity) : base(capacity)
    {
        _accessOrder = new LinkedList<CacheEntry>();
        _lookup = new Dictionary<TKey, LinkedListNode<CacheEntry>>(capacity);
    }

    public override string Name => "LRU";

    public override int Count => _lookup.Count;

    public override bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (!_lookup.TryGetValue(key, out var node))
        {
            value = default;
            return false;
        }

        // Hit: promote the node to head so it becomes most recently used.
        MoveToHead(node);
        value = node.Value.Value;
        return true;
    }

    public override void Put(TKey key, TValue value)
    {
        if (_lookup.TryGetValue(key, out var existing))
        {
            // Update in place and promote — a write counts as a use.
            existing.Value = new CacheEntry(key, value);
            MoveToHead(existing);
            return;
        }

        if (_lookup.Count >= Capacity)
        {
            EvictLeastRecentlyUsed();
        }

        var node = _accessOrder.AddFirst(new CacheEntry(key, value));
        _lookup[key] = node;
    }

    public override bool Contains(TKey key) => _lookup.ContainsKey(key);

    public override bool Remove(TKey key)
    {
        if (!_lookup.Remove(key, out var node))
        {
            return false;
        }

        _accessOrder.Remove(node);
        return true;
    }

    public override void Clear()
    {
        _lookup.Clear();
        _accessOrder.Clear();
    }

    private void MoveToHead(LinkedListNode<CacheEntry> node)
    {
        // Removing and re-adding the same node instance is O(1):
        // LinkedList<T> mutates pointers without allocating a new node.
        _accessOrder.Remove(node);
        _accessOrder.AddFirst(node);
    }

    private void EvictLeastRecentlyUsed()
    {
        var tail = _accessOrder.Last
            ?? throw new InvalidOperationException("Cache is full but list is empty.");

        _accessOrder.RemoveLast();
        _lookup.Remove(tail.Value.Key);
        OnEvicted(tail.Value.Key);
    }

    // Pairs key and value together so the linked list can carry both.
    // A struct (not class) avoids an extra allocation per entry.
    private readonly record struct CacheEntry(TKey Key, TValue Value);
}