using System.Threading.Tasks;

namespace CommandQuery.Framing
{
    public interface ICommandBroker
    {
        Task<TResponse> ExecuteAsync<TRequest,TResponse>(TRequest message) where TRequest : ICommandMessage;
        TResponse Execute<TRequest,TResponse>(TRequest message) where TRequest : ICommandMessage;
        Task<TResponse> QueryAsync<TRequest, TResponse>(TRequest message) where TRequest : IQueryMessage;
        TResponse Query<TRequest, TResponse>(TRequest message) where TRequest : IQueryMessage;
    }
}