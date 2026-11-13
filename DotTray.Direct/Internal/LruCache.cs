namespace DotTray.Direct.Internal;

using System.Diagnostics.CodeAnalysis;

internal sealed class LruCache<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, LinkedListNode<Entry>> _nodes;
    private readonly LinkedList<Entry> _order;
    private readonly Action<TKey, TValue>? _onRemoved;

    public int Count => _nodes.Count;

    public int Capacity { get; }

    public LruCache(int capacity, Action<TKey, TValue>? onRemoved = null, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(capacity, 0, nameof(capacity));

        Capacity = capacity;
        _nodes = new Dictionary<TKey, LinkedListNode<Entry>>(capacity, comparer);
        _order = [];
        _onRemoved = onRemoved;
    }

    public bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (!_nodes.TryGetValue(key, out var node))
        {
            value = default;
            return false;
        }

        MoveToFront(node);

        value = node.Value.Value;
        return true;
    }

    public void Add(TKey key, TValue value)
    {
        var entry = new Entry(key, value);

        if (_nodes.TryGetValue(key, out var existing))
        {
            var prev = existing.Value.Value;

            existing.Value = entry;
            MoveToFront(existing);

            if (!EqualityComparer<TValue>.Default.Equals(prev, value)) _onRemoved?.Invoke(key, prev);

            return;
        }

        if (_nodes.Count < Capacity)
        {
            var node = new LinkedListNode<Entry>(entry);

            _order.AddFirst(node);
            _nodes[key] = node;
            return;
        }

        var last = _order.Last!;
        var evicted = last.Value;

        _order.RemoveLast();
        _nodes.Remove(evicted.Key);

        last.Value = entry;
        _order.AddFirst(last);
        _nodes[key] = last;

        _onRemoved?.Invoke(evicted.Key, evicted.Value);
    }

    public bool Remove(TKey key)
    {
        if (!_nodes.Remove(key, out var node)) return false;

        _order.Remove(node);
        _onRemoved?.Invoke(key, node.Value.Value);

        return true;
    }

    public void Clear()
    {
        if (_onRemoved is not null)
        {
            for (var node = _order.First; node is not null; node = node.Next)
            {
                _onRemoved(node.Value.Key, node.Value.Value);
            }
        }

        _nodes.Clear();
        _order.Clear();
    }

    private void MoveToFront(LinkedListNode<Entry> node)
    {
        if (ReferenceEquals(_order.First, node)) return;

        _order.Remove(node);
        _order.AddFirst(node);
    }

    private readonly record struct Entry
    {
        public readonly TKey Key { get; }
        public readonly TValue Value { get; }

        public Entry(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}