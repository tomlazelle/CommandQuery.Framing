using System;

namespace CommandQuery.Framing
{
    /// <summary>
    /// Represents the response from executing a command or query.
    /// </summary>
    /// <typeparam name="T">The type of data returned in the response.</typeparam>
    public class CommandResponse<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the response data.
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Gets or sets a message describing the result (typically used for errors).
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Gets or sets the exception that occurred, if any.
        /// </summary>
        public Exception Exception { get; set; }
    }
}