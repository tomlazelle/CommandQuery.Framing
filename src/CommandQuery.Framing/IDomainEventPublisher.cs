using System;
using System.Threading;
using System.Threading.Tasks;

namespace CommandQuery.Framing;

public interface IDomainEventPublisher
{
    event EventHandler MessageSent;
    event EventHandler<DomainEventArgs> MessageResult;
    Task Publish<TMessageType>(TMessageType message, CancellationToken cancellationToken);
}