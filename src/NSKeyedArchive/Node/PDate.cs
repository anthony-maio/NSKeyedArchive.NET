namespace NSKeyedArchive
{
    /// <summary>
    /// Represents a date value in a property list.
    /// </summary>
    public class PDate : PNode
    {
        /// <summary>
        /// Gets or sets the date value. All dates are stored in UTC.
        /// </summary>
        public DateTime Value { get; set; }

        /// <inheritdoc/>
        public override PNodeType NodeType => PNodeType.Date;

        /// <inheritdoc/>
        public override T GetValue<T>()
        {
            if (typeof(T) == typeof(DateTime))
                return (T)(object)Value;
            return base.GetValue<T>();
        }

        /// <inheritdoc/>
        public override string ToString() => Value.ToString("O");
    }
}
