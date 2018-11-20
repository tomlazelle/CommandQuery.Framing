using System.Threading.Tasks;

namespace CommandQuery.Framing
{
    public interface IAsyncQueryHandler<TMessageType, TReturnType>
    {
        Task<TReturnType> Execute(TMessageType message);
    }
}