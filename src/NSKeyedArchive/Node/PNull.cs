namespace NSKeyedArchive
{
    /// <summary>
    /// Represents a null value in a property list.
    /// </summary>
    public class PNull : PNode
    {
        /// <inheritdoc/>
        public override PNodeType NodeType => PNodeType.Null;

        /// <inheritdoc/>
        public override string ToString() => "null";
    }
}
