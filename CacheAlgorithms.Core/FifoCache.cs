using System.Diagnostics.CodeAnalysis;

namespace CacheAlgorithms.Core;

/// <summary>
/// Fixed-capacity cache with First-In-First-Out eviction policy.
/// Evicts the entry that was inserted earliest, regardless of access patterns.
/// All operations run in amortized O(1) time.
/// </summary>
public sealed class FifoCache<TKey, TValue> : CacheStrategyBase<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _storage;
    private readonly Queue<TKey> _insertionOrder;

    public FifoCache(int capacity) : base(capacity)
    {
        _storage = new Dictionary<TKey, TValue>(capacity);
        _insertionOrder = new Queue<TKey>(capacity);
    }

    public override string Name => "FIFO";

    public override int Count => _storage.Count;

    public override bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue value)
        => _storage.TryGetValue(key, out value);

    public override void Put(TKey key, TValue value)
    {
        // FIFO orders by insertion, not by write — overwriting an existing key
        // must not change its queue position.
        if (_storage.ContainsKey(key))
        {
            _storage[key] = value;
            return;
        }

        if (_storage.Count >= Capacity)
        {
            EvictOldest();
        }

        _storage[key] = value;
        _insertionOrder.Enqueue(key);
    }

    public override bool Contains(TKey key) => _storage.ContainsKey(key);

    public override bool Remove(TKey key) => _storage.Remove(key);
    // Note: stale keys remain in _insertionOrder and are filtered lazily in EvictOldest().

    public override void Clear()
    {
        _storage.Clear();
        _insertionOrder.Clear();
    }

    private void EvictOldest()
    {
        // Skip "ghost" keys left in the queue by previous Remove() calls.
        // Removing from the middle of Queue<T> is O(n), so we defer cleanup
        // until eviction time and accept that the queue may be longer than _storage.
        while (_insertionOrder.TryDequeue(out var oldestKey))
        {
            if (_storage.Remove(oldestKey))
            {
                OnEvicted(oldestKey);
                return;
            }
        }
    }
}