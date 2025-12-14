using System;
using System.Threading.Tasks;

namespace CommandQuery.Framing
{
    /// <summary>
    /// Represents a domain event handler that processes messages of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of message this domain event handles.</typeparam>
    public interface IDomainEvent<in T>
    {
        /// <summary>
        /// Event raised when the domain event execution completes.
        /// </summary>
        event EventHandler<DomainEventArgs>? OnComplete;
        
        /// <summary>
        /// Executes the domain event handler for the given message.
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Execute(T message);
    }

    /// <summary>
    /// Event arguments for domain event completion notifications.
    /// </summary>
    public class DomainEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether the domain event was processed successfully.
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Gets or sets a message describing the result of the domain event execution.
        /// </summary>
        public string Message { get; set; }
    }
}