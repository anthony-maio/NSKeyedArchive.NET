using System;
using NSKeyedArchive;
using System.Reflection;

namespace NSKeyedArchive
{
    /// <summary>
    /// Exception thrown when there is an error processing a property list.
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
        /// Initializes a new instance of the <see cref="PListFormatException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="inner">The inner exception.</param>
        public NSKeyedUnarchiverException(string message, Exception inner) : base(message, inner) { }
     }
}
