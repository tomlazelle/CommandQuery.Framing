using System.Threading.Tasks;

namespace CommandQuery.Framing
{
    public interface IAsyncCommandHandler<TRequest,TResponse>
    {
        Task<TResponse> Execute(TRequest message);
    }
}