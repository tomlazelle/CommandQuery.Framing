using System;
using System.Threading;
using System.Threading.Tasks;

namespace CommandQuery.Framing;

/// <summary>
/// Publisher for domain events that notifies registered handlers.
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>
    /// Event raised when a message is sent to domain event handlers.
    /// </summary>
    event EventHandler MessageSent;
    
    /// <summary>
    /// Event raised when domain event handlers complete processing.
    /// </summary>
    event EventHandler<DomainEventArgs> MessageResult;
    
    /// <summary>
    /// Publishes a message to all registered domain event handlers.
    /// </summary>
    /// <typeparam name="TMessageType">The type of message to publish.</typeparam>
    /// <param name="message">The message to publish to domain event handlers.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous publish operation.</returns>
    Task Publish<TMessageType>(TMessageType message, CancellationToken cancellationToken);
}