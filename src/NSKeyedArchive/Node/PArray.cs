using System.Collections;

namespace NSKeyedArchive
{
    /// <summary>
    /// Represents an array of nodes in a property list.
    /// </summary>
    public class PArray : PNode, IList<PNode>
    {
        private readonly List<PNode> _items = new();

        /// <inheritdoc/>
        public override PNodeType NodeType => PNodeType.Array;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="PArray"/>.
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="PArray"/> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets or sets the <see cref="PNode"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The <see cref="PNode"/> at the specified index.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
        public PNode this[int index]
        {
            get => _items[index];
            set => _items[index] = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Adds a <see cref="PNode"/> to the end of the <see cref="PArray"/>.
        /// </summary>
        /// <param name="item">The <see cref="PNode"/> to be added.</param>
        /// <exception cref="ArgumentNullException">Thrown when the item is null.</exception>
        public void Add(PNode item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            _items.Add(item);
        }

        /// <summary>
        /// Removes all elements from the <see cref="PArray"/>.
        /// </summary>
        public void Clear() => _items.Clear();

        /// <summary>
        /// Determines whether the <see cref="PArray"/> contains a specific value.
        /// </summary>
        /// <param name="item">The <see cref="PNode"/> to locate in the <see cref="PArray"/>.</param>
        /// <returns>true if item is found in the <see cref="PArray"/>; otherwise, false.</returns>
        public bool Contains(PNode item) => _items.Contains(item);

        /// <summary>
        /// Copies the elements of the <see cref="PArray"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="PArray"/>. The <see cref="Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(PNode[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        /// <summary>
        /// Determines the index of a specific item in the <see cref="PArray"/>.
        /// </summary>
        /// <param name="item">The <see cref="PNode"/> to locate in the <see cref="PArray"/>.</param>
        /// <returns>The index of item if found in the list; otherwise, -1.</returns>
        public int IndexOf(PNode item) => _items.IndexOf(item);

        /// <summary>
        /// Inserts an item to the <see cref="PArray"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The <see cref="PNode"/> to insert into the <see cref="PArray"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when the item is null.</exception>
        public void Insert(int index, PNode item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            _items.Insert(index, item);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="PArray"/>.
        /// </summary>
        /// <param name="item">The <see cref="PNode"/> to remove from the <see cref="PArray"/>.</param>
        /// <returns>true if item was successfully removed from the <see cref="PArray"/>; otherwise, false. This method also returns false if item is not found in the original <see cref="PArray"/>.</returns>
        public bool Remove(PNode item) => _items.Remove(item);

        /// <summary>
        /// Removes the <see cref="PNode"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index) => _items.RemoveAt(index);

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="PArray"/>.
        /// </summary>
        /// <returns>A <see cref="IEnumerator{PNode}"/> for the <see cref="PArray"/>.</returns>
        public IEnumerator<PNode> GetEnumerator() => _items.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public override string ToString() => $"Array[{Count}]";
    }
}
