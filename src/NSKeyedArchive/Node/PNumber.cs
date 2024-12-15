namespace NSKeyedArchive
{
    /// <summary>
    /// Represents a numeric value in a property list. Uses decimal for maximum precision
    /// while handling both integer and floating-point values from the property list.
    /// </summary>
    public class PNumber : PNode
    {
        /// <summary>
        /// Gets or sets the numeric value.
        /// </summary>
        public decimal Value { get; set; }

        /// <inheritdoc/>
        public override PNodeType NodeType => PNodeType.Number;

        /// <inheritdoc/>
        public override T GetValue<T>()
        {
            // Support conversion to various numeric types
            if (typeof(T) == typeof(decimal))
                return (T)(object)Value;
            if (typeof(T) == typeof(double))
                return (T)(object)(double)Value;
            if (typeof(T) == typeof(float))
                return (T)(object)(float)Value;
            if (typeof(T) == typeof(long))
                return (T)(object)(long)Value;
            if (typeof(T) == typeof(int))
                return (T)(object)(int)Value;
            if (typeof(T) == typeof(short))
                return (T)(object)(short)Value;
            if (typeof(T) == typeof(byte))
                return (T)(object)(byte)Value;
            return base.GetValue<T>();
        }

        /// <inheritdoc/>
        public override string ToString() => Value.ToString();
    }
}
