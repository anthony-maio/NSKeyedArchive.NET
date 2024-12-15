namespace NSKeyedArchive
{
    /// <summary>
    /// Represents the type of a property list node.
    /// </summary>
    public enum PNodeType
    {
        /// <summary>
        /// Represents a string value (NSString).
        /// </summary>
        String,

        /// <summary>
        /// Represents a number value (NSNumber).
        /// </summary>
        Number,

        /// <summary>
        /// Represents a boolean value (NSNumber with a boolean value).
        /// </summary>
        Boolean,

        /// <summary>
        /// Represents a date value (NSDate).
        /// </summary>
        Date,

        /// <summary>
        /// Represents binary data (NSData).
        /// </summary>
        Data,

        /// <summary>
        /// Represents an array of values (NSArray).
        /// </summary>
        Array,

        /// <summary>
        /// Represents a dictionary of key-value pairs (NSDictionary).
        /// </summary>
        Dictionary,

        /// <summary>
        /// Represents a null value (NSNull).
        /// </summary>
        Null
    }
}

