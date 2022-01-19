using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CommandQuery.Framing
{
    public static class CommandQueryExtensions
    {
        public static IServiceCollection AddCommandQuery(this IServiceCollection serviceCollection, params Assembly[] handlers)
        {

            serviceCollection.AddSingleton<IBroker, Broker>();
            serviceCollection.AddSingleton<IDomainEventPublisher, DomainEventPublisher>();

            serviceCollection.ScanAndAddTransientTypes(handlers,
                                                       new Type[]
                                                       {
                                                           typeof(IAsyncHandler<,>),
                                                           typeof(IHandler<,>),
                                                           typeof(IDomainEvent<>)
                                                       });

            return serviceCollection;
        }
    }
}