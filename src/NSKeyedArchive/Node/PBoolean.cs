namespace NSKeyedArchive
{
    /// <summary>
    /// Represents a boolean value in a property list.
    /// </summary>
    public class PBoolean : PNode
    {
        /// <summary>
        /// Gets or sets the boolean value.
        /// </summary>
        public bool Value { get; set; }

        /// <inheritdoc/>
        public override PNodeType NodeType => PNodeType.Boolean;

        /// <inheritdoc/>
        public override T GetValue<T>()
        {
            if (typeof(T) == typeof(bool))
                return (T)(object)Value;
            return base.GetValue<T>();
        }

        /// <inheritdoc/>
        public override string ToString() => Value.ToString();
    }
}
