namespace CommandQuery.Framing
{
    /// <summary>
    /// Configuration options for CommandQuery framework registration.
    /// </summary>
    public class CommandQueryOptions
    {
        /// <summary>
        /// When true, logs all registered handlers and domain events to the console.
        /// Default is false.
        /// </summary>
        public bool LogRegistrations { get; set; }

        /// <summary>
        /// When true, validates that all handler types in the scanned assemblies 
        /// have been successfully registered and throws an exception if any are missing.
        /// Default is false.
        /// </summary>
        public bool ValidateRegistrations { get; set; }
    }
}
