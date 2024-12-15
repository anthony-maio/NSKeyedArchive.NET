namespace NSKeyedArchive
{
    /// <summary>
    /// Represents binary data in a property list.
    /// </summary>
    public class PData : PNode
    {
        /// <summary>
        /// Gets or sets the binary data.
        /// </summary>
        public byte[] Value { get; set; } = Array.Empty<byte>();

        /// <inheritdoc/>
        public override PNodeType NodeType => PNodeType.Data;

        /// <inheritdoc/>
        public override T GetValue<T>()
        {
            if (typeof(T) == typeof(byte[]))
                return (T)(object)Value;
            return base.GetValue<T>();
        }

        /// <inheritdoc/>
        public override string ToString() => $"<{Value.Length} bytes>";
    }
}
