using System.Threading.Tasks;

namespace CommandQuery.Framing
{
    public abstract class AsyncCommandHandler<TRequest,TResponse> : IAsyncCommandHandler<TRequest,TResponse>
    {
        protected readonly IDomainEventPublisher DomainEventPublisher;

        protected AsyncCommandHandler(IDomainEventPublisher domainEventPublisher)
        {
            DomainEventPublisher = domainEventPublisher;
        }

        public abstract Task<TResponse> Execute(TRequest message);
    }
}