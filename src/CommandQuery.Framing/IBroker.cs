using System.Threading;
using System.Threading.Tasks;

namespace CommandQuery.Framing
{
    public interface IBroker
    {
        Task<TResponse> HandleAsync<TRequest, TResponse>(TRequest message, CancellationToken cancellationToken = default) where TRequest : IMessage;
        TResponse Handle<TRequest, TResponse>(TRequest message) where TRequest : IMessage;
    }
}