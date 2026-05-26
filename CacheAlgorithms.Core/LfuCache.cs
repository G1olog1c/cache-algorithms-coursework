using System.Diagnostics.CodeAnalysis;

namespace CacheAlgorithms.Core;

/// <summary>
/// Fixed-capacity cache with Least Frequently Used eviction policy.
/// When the cache is full, evicts the entry that has been accessed the fewest times.
/// Ties between entries with the same frequency are broken by LRU order.
/// All operations run in O(1) time using the bucket-based scheme described by
/// Pasquale et al., 2010.
/// </summary>
public sealed class LfuCache<TKey, TValue> : CacheStrategyBase<TKey, TValue>
    where TKey : notnull
{
    // Frequency buckets, ordered by frequency ascending.
    // _freqBuckets.First always points to the lowest-frequency bucket — the eviction candidate.
    private readonly LinkedList<FrequencyBucket> _freqBuckets;

    // Direct lookup from key to its tracking entry.
    private readonly Dictionary<TKey, Entry> _lookup;

    public LfuCache(int capacity) : base(capacity)
    {
        _freqBuckets = new LinkedList<FrequencyBucket>();
        _lookup = new Dictionary<TKey, Entry>(capacity);
    }

    public override string Name => "LFU";

    public override int Count => _lookup.Count;

    public override bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (!_lookup.TryGetValue(key, out var entry))
        {
            value = default;
            return false;
        }

        IncrementFrequency(entry);
        value = entry.Value;
        return true;
    }

    public override void Put(TKey key, TValue value)
    {
        if (_lookup.TryGetValue(key, out var existing))
        {
            // Writing to an existing key counts as a use — bump its frequency.
            existing.Value = value;
            IncrementFrequency(existing);
            return;
        }

        if (_lookup.Count >= Capacity)
        {
            EvictLeastFrequentlyUsed();
        }

        // New entries start at frequency 1.
        var bucket = GetOrCreateBucketAtHead(frequency: 1);
        var keyNode = bucket.Value.Keys.AddFirst(key);
        _lookup[key] = new Entry(value, bucket, keyNode);
    }

    public override bool Contains(TKey key) => _lookup.ContainsKey(key);

    public override bool Remove(TKey key)
    {
        if (!_lookup.Remove(key, out var entry))
        {
            return false;
        }

        entry.Bucket.Value.Keys.Remove(entry.KeyNode);
        RemoveBucketIfEmpty(entry.Bucket);
        return true;
    }

    public override void Clear()
    {
        _lookup.Clear();
        _freqBuckets.Clear();
    }

    private void IncrementFrequency(Entry entry)
    {
        var oldBucket = entry.Bucket;
        var newFrequency = oldBucket.Value.Frequency + 1;

        // Remove the key from its current bucket.
        oldBucket.Value.Keys.Remove(entry.KeyNode);

        // The bucket for newFrequency, if it exists, must be the immediate neighbour
        // to the right — because frequencies grow by exactly 1 and buckets stay sorted.
        var newBucket = (oldBucket.Next is { } next && next.Value.Frequency == newFrequency)
            ? next
            : _freqBuckets.AddAfter(oldBucket, new FrequencyBucket(newFrequency));

        var newKeyNode = newBucket.Value.Keys.AddFirst(entry.KeyNode.Value);

        entry.Bucket = newBucket;
        entry.KeyNode = newKeyNode;

        RemoveBucketIfEmpty(oldBucket);
    }

    private void EvictLeastFrequentlyUsed()
    {
        // First bucket has the lowest frequency. Within it, the last key is the
        // least recently used among the equally-rare candidates.
        var minBucket = _freqBuckets.First
            ?? throw new InvalidOperationException("Cache is full but no buckets exist.");

        var victimNode = minBucket.Value.Keys.Last
            ?? throw new InvalidOperationException("Empty bucket was not removed.");

        var victimKey = victimNode.Value;
        minBucket.Value.Keys.RemoveLast();
        _lookup.Remove(victimKey);

        RemoveBucketIfEmpty(minBucket);
        OnEvicted(victimKey);
    }

    private LinkedListNode<FrequencyBucket> GetOrCreateBucketAtHead(int frequency)
    {
        if (_freqBuckets.First is { } first && first.Value.Frequency == frequency)
        {
            return first;
        }

        return _freqBuckets.AddFirst(new FrequencyBucket(frequency));
    }

    private void RemoveBucketIfEmpty(LinkedListNode<FrequencyBucket> bucket)
    {
        if (bucket.Value.Keys.Count == 0)
        {
            _freqBuckets.Remove(bucket);
        }
    }

    // A bucket groups all keys sharing the same access frequency.
    // Keys inside the bucket are ordered by recency (head = most recent),
    // which gives LRU tie-breaking when several keys share the minimum frequency.
    private sealed class FrequencyBucket
    {
        public FrequencyBucket(int frequency)
        {
            Frequency = frequency;
            Keys = new LinkedList<TKey>();
        }

        public int Frequency { get; }

        public LinkedList<TKey> Keys { get; }
    }

    // Per-key metadata. Keeps direct pointers to its bucket and its node inside
    // that bucket, so frequency bumps and removals are O(1).
    private sealed class Entry
    {
        public Entry(TValue value, LinkedListNode<FrequencyBucket> bucket, LinkedListNode<TKey> keyNode)
        {
            Value = value;
            Bucket = bucket;
            KeyNode = keyNode;
        }

        public TValue Value { get; set; }

        public LinkedListNode<FrequencyBucket> Bucket { get; set; }

        public LinkedListNode<TKey> KeyNode { get; set; }
    }
}