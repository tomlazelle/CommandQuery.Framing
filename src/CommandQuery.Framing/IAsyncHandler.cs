using System.Threading;
using System.Threading.Tasks;

namespace CommandQuery.Framing
{
    /// <summary>
    /// Asynchronous request handler for processing commands and queries.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request message.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public interface IAsyncHandler<in TRequest, TResponse> where TRequest : IMessage
    {
        /// <summary>
        /// Executes the handler asynchronously for the given request message.
        /// </summary>
        /// <param name="message">The request message to process.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        /// <returns>A task representing the asynchronous operation, containing the response.</returns>
        Task<TResponse> Execute(TRequest message, CancellationToken cancellationToken);
    }
}