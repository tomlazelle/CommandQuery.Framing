using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace CommandQuery.Framing
{
    public class DomainEventPublisher : IDomainEventPublisher
    {
        private readonly IServiceProvider _serviceProvider;

        public DomainEventPublisher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Publish<TMessageType>(TMessageType message)
        {
            var events = _serviceProvider.GetServices<IDomainEvent<TMessageType>>();

            foreach (var domainEvent in events)
            {
                Task.Run(() => domainEvent.Execute(message));
            }
        }
    }
}