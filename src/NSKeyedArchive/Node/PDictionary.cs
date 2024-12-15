using System.Collections;

namespace NSKeyedArchive
{
    /// <summary>
    /// Represents a dictionary of key-value pairs in a property list.
    /// </summary>
    public class PDictionary : PNode, IDictionary<string, PNode>
    {
        private readonly Dictionary<string, PNode> _items = [];

        /// <summary>
        ///  Represents a dictionary of key-value pairs in a property list.
        /// </summary>
        public PDictionary()
        {
        }

        /// <summary>
        ///  Represents a dictionary of key-value pairs in a property list.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public PDictionary(PDictionary dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));
            foreach (var item in dictionary)
                _items.Add(item.Key, item.Value);
        }
        /// <inheritdoc/>
        public override PNodeType NodeType => PNodeType.Dictionary;
        /// <inheritdoc/>
        public int Count => _items.Count;
        /// <inheritdoc/>
        public bool IsReadOnly => false;
        /// <inheritdoc/>
        public ICollection<string> Keys => _items.Keys;
        /// <inheritdoc/>
        public ICollection<PNode> Values => _items.Values;
        /// <inheritdoc/>
        public PNode this[string key]
        {
            get => _items[key];
            set => _items[key] = value ?? throw new ArgumentNullException(nameof(value));
        }
        /// <inheritdoc/>
        public void Add(string key, PNode value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            _items.Add(key, value);
        }
        /// <inheritdoc/>
        public void Add(KeyValuePair<string, PNode> item) => Add(item.Key, item.Value);
        /// <inheritdoc/>
        public void Clear() => _items.Clear();
        /// <inheritdoc/>
        public bool Contains(KeyValuePair<string, PNode> item) => _items.Contains(item);
        /// <inheritdoc/>
        public bool ContainsKey(string key) => _items.ContainsKey(key);
        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<string, PNode>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, PNode>>)_items).CopyTo(array, arrayIndex);
        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, PNode>> GetEnumerator() => _items.GetEnumerator();
        /// <inheritdoc/>
        public bool Remove(string key) => _items.Remove(key);
        /// <inheritdoc/>
        public bool Remove(KeyValuePair<string, PNode> item) => ((ICollection<KeyValuePair<string, PNode>>)_items).Remove(item);
        /// <inheritdoc/>
        public bool TryGetValue(string key, out PNode value) => _items.TryGetValue(key, out value);
        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        /// <inheritdoc/>
        public override string ToString() => $"Dictionary[{Count}]";
    }
}
