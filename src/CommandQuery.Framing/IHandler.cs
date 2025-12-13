namespace CommandQuery.Framing
{
    /// <summary>
    /// Synchronous request handler for processing commands and queries.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request message.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public interface IHandler<in TRequest, out TResponse> where TRequest : IMessage
    {
        /// <summary>
        /// Executes the handler synchronously for the given request message.
        /// </summary>
        /// <param name="message">The request message to process.</param>
        /// <returns>The response from executing the handler.</returns>
        TResponse Execute(TRequest message);
    }
}