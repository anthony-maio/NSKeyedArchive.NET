using System;
using NSKeyedArchive;
using System.Reflection;

namespace NSKeyedArchive
{
    /// <summary>
    /// Exception thrown when there is an error processing a property list (base)
    /// </summary>
    public class PListException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PListException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="inner">The inner exception.</param>
        public PListException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PListException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public PListException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception thrown when there is an error in the format of a property list.
    /// </summary>
    public class PListFormatException : PListException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PListFormatException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public PListFormatException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PListFormatException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="inner">The inner exception.</param>
        public PListFormatException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Exception thrown when there is an error unarchiving an NSKeyedArchiver plist.
    /// </summary>
    public class NSKeyedUnarchiverException : PListException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NSKeyedUnarchiverException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public NSKeyedUnarchiverException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NSKeyedUnarchiverException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="inner">The inner exception.</param>
        public NSKeyedUnarchiverException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Represents an error encountered while decoding an NSKeyedArchive.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="NSArchiveException"/> class.
    /// </remarks>
    /// <param name="message">The error message.</param>
    /// <param name="nodeKey">The key of the problematic node, if any.</param>
    /// <param name="problemNode">The problematic node, if any.</param>
    public class NSArchiveException(string message, string? nodeKey = null, PNode? problemNode = null)
        : Exception(message)
    {

        /// <summary>
        /// Gets the key of the problematic node, if any.
        /// </summary>
        public string? NodeKey { get; } = nodeKey;

        /// <summary>
        /// Gets the problematic node, if any.
        /// </summary>
        public PNode? ProblemNode { get; } = problemNode;

        /// <summary>
        /// Returns a string representation of the exception.
        /// </summary>
        /// <returns>A string representation of the exception.</returns>
        public override string ToString()
        {
            return $"{Message} (Key: {NodeKey}, Node: {ProblemNode})";
        }
    }

    /// <summary>
    /// Represents an error where the recursion depth limit was exceeded.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="NSArchiveRecursionException"/> class.
    /// </remarks>
    /// <param name="depth">The recursion depth at which the error occurred.</param>
    /// <param name="nodeKey">The key of the problematic node, if any.</param>
    /// <param name="problemNode">The problematic node, if any.</param>
    public class NSArchiveRecursionException(int depth, string? nodeKey = null, PNode? problemNode = null)
        : NSArchiveException($"Recursion depth limit exceeded at depth {depth}.", nodeKey, problemNode)
    {

        /// <summary>
        /// Gets the recursion depth at which the error occurred.
        /// </summary>
        public int RecursionDepth { get; } = depth;
    }

    /// <summary>
    /// Represents an error where a malformed or unexpected node was encountered.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="NSArchiveMalformedNodeException"/> class.
    /// </remarks>
    /// <param name="message">The error message.</param>
    /// <param name="nodeKey">The key of the problematic node, if any.</param>
    /// <param name="problemNode">The problematic node, if any.</param>
    public class NSArchiveMalformedNodeException(string message, string? nodeKey = null, PNode? problemNode = null)
        : NSArchiveException(message, nodeKey, problemNode)
    {
    }
}
