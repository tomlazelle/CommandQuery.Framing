using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace CommandQuery.Framing
{
    public class DomainEventPublisher : IDomainEventPublisher
    {
        public event EventHandler MessageSent;
        public event EventHandler<DomainEventArgs> MessageResult;

        private readonly IServiceProvider _serviceProvider;

        public DomainEventPublisher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Publish<TMessageType>(TMessageType message)
        {
            var events = _serviceProvider.GetServices<IDomainEvent<TMessageType>>();

            foreach (var domainEvent in events)
            {
                domainEvent.OnComplete += DomainEvent_OnComplete;
                MessageSent?.Invoke(this, new DomainEventArgs());
                await domainEvent.Execute(message);
            }
        }

        private void DomainEvent_OnComplete(object sender, DomainEventArgs e)
        {
            MessageResult?.Invoke(this, e);
        }
    }
}