namespace NSKeyedArchive
{
    /// <summary>
    /// Represents a string value in a property list.
    /// </summary>
    public class PString : PNode
    {
        /// <summary>
        /// Gets or sets the string value.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <inheritdoc/>
        public override PNodeType NodeType => PNodeType.String;

        /// <inheritdoc/>
        public override T GetValue<T>()
        {
            if (typeof(T) == typeof(string))
                return (T)(object)Value;
            return base.GetValue<T>();
        }

        /// <inheritdoc/>
        public override string ToString() => Value;
    }
}
