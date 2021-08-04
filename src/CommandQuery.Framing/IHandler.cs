namespace CommandQuery.Framing
{
    /// <summary>
    /// Synchronous request handler
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public interface IHandler<in TRequest, TResponse> where TRequest : IRqstMessage
    {
        TResponse Execute(TRequest message);
    }
}