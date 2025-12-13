using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CommandQuery.Framing
{
    /// <summary>
    /// Extension methods for configuring CommandQuery services in dependency injection.
    /// </summary>
    public static class CommandQueryExtensions
    {
        /// <summary>
        /// Adds CommandQuery framework services to the dependency injection container.
        /// Registers the broker, domain event publisher, and scans assemblies for handlers and domain events.
        /// </summary>
        /// <param name="serviceCollection">The service collection to add services to.</param>
        /// <param name="handlers">Assemblies to scan for handlers (IHandler, IAsyncHandler) and domain events (IDomainEvent).</param>
        /// <returns>The service collection for chaining.</returns>
        /// <example>
        /// <code>
        /// services.AddCommandQuery(typeof(Startup).Assembly);
        /// </code>
        /// </example>
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