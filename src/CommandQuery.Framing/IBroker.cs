using System.Threading;
using System.Threading.Tasks;

namespace CommandQuery.Framing
{
    /// <summary>
    /// Central broker for dispatching commands and queries to their registered handlers.
    /// </summary>
    public interface IBroker
    {
        /// <summary>
        /// Asynchronously executes a handler for the given request message.
        /// </summary>
        /// <typeparam name="TRequest">The type of request message implementing <see cref="IMessage"/>.</typeparam>
        /// <typeparam name="TResponse">The type of response returned by the handler.</typeparam>
        /// <param name="message">The request message to handle. Cannot be null.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The response from the handler.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when message is null.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when no handler is registered for the request type.</exception>
        Task<TResponse> HandleAsync<TRequest, TResponse>(TRequest message, CancellationToken cancellationToken) where TRequest : IMessage;
        
        /// <summary>
        /// Synchronously executes a handler for the given request message.
        /// </summary>
        /// <typeparam name="TRequest">The type of request message implementing <see cref="IMessage"/>.</typeparam>
        /// <typeparam name="TResponse">The type of response returned by the handler.</typeparam>
        /// <param name="message">The request message to handle. Cannot be null.</param>
        /// <returns>The response from the handler.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when message is null.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when no handler is registered for the request type.</exception>
        TResponse Handle<TRequest, TResponse>(TRequest message) where TRequest : IMessage;
    }
}