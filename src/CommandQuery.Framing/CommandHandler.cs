using System.Threading.Tasks;

namespace CommandQuery.Framing
{
    public abstract class CommandHandler<TRequest,TResponse> : ICommandHandler<TRequest,TResponse>
    {
        protected readonly IDomainEventPublisher DomainEventPublisher;

        protected CommandHandler(IDomainEventPublisher domainEventPublisher)
        {
            DomainEventPublisher = domainEventPublisher;
        }
        
        public abstract TResponse Execute(TRequest message);
    }
}