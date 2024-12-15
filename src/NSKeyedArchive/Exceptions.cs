namespace NSKeyedArchive
{
    /// <summary>
    /// Exception thrown when there is an error processing a property list.
    /// </summary>
    // TODO : flesh this out, make this abstract, possible capture information on what happened.
    public class PListException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PListException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public PListException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PListException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public PListException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Exception thrown when there is a format error in a property list.
    /// </summary>
    public class PListFormatException : PListException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PListFormatException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public PListFormatException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PListFormatException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public PListFormatException(string message, Exception inner) : base(message, inner) { }
    }
}

