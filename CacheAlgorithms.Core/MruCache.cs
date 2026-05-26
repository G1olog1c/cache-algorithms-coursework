using System.Diagnostics.CodeAnalysis;

namespace CacheAlgorithms.Core;

/// <summary>
/// Fixed-capacity cache with Most Recently Used eviction policy.
/// When the cache is full, evicts the entry that was most recently accessed —
/// the opposite of LRU. This policy outperforms LRU on workloads that scan
/// repeatedly through a set larger than the cache (e.g. cyclic table scans),
/// where LRU evicts entries just before they would be reused.
/// All operations run in O(1) time.
/// </summary>
public sealed class MruCache<TKey, TValue> : CacheStrategyBase<TKey, TValue>
    where TKey : notnull
{
    private readonly LinkedList<CacheEntry> _accessOrder;
    private readonly Dictionary<TKey, LinkedListNode<CacheEntry>> _lookup;

    public MruCache(int capacity) : base(capacity)
    {
        _accessOrder = new LinkedList<CacheEntry>();
        _lookup = new Dictionary<TKey, LinkedListNode<CacheEntry>>(capacity);
    }

    public override string Name => "MRU";

    public override int Count => _lookup.Count;

    public override bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (!_lookup.TryGetValue(key, out var node))
        {
            value = default;
            return false;
        }

        MoveToHead(node);
        value = node.Value.Value;
        return true;
    }

    public override void Put(TKey key, TValue value)
    {
        if (_lookup.TryGetValue(key, out var existing))
        {
            existing.Value = new CacheEntry(key, value);
            MoveToHead(existing);
            return;
        }

        if (_lookup.Count >= Capacity)
        {
            EvictMostRecentlyUsed();
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
        _accessOrder.Remove(node);
        _accessOrder.AddFirst(node);
    }

    private void EvictMostRecentlyUsed()
    {
        // The only structural difference from LRU: we evict from the head
        // (most recent) instead of the tail (least recent).
        var head = _accessOrder.First
            ?? throw new InvalidOperationException("Cache is full but list is empty.");

        _accessOrder.RemoveFirst();
        _lookup.Remove(head.Value.Key);
        OnEvicted(head.Value.Key);
    }

    private readonly record struct CacheEntry(TKey Key, TValue Value);
}