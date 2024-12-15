using System;
using System.Collections.Generic;

namespace NSKeyedArchive
{
    /// <summary>
    /// Base class for all property list value types. This provides a common interface
    /// for working with different kinds of plist values while maintaining type safety.
    /// </summary>
    public abstract class PNode
    {
        /// <summary>
        /// Gets the type of this node.
        /// </summary>
        public abstract PNodeType NodeType { get; }

        /// <summary>
        /// Attempts to get the value of this node as the specified type.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <returns>The converted value.</returns>
        /// <exception cref="InvalidCastException">If the conversion is not possible.</exception>
        public virtual T GetValue<T>()
        {
            throw new InvalidCastException($"Cannot convert {NodeType} to {typeof(T)}");
        }
    }
}
