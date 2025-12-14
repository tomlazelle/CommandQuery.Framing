using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace CommandQuery.Framing
{
    /// <summary>
    /// Central broker for dispatching commands and queries to their registered handlers.
    /// </summary>
    public class Broker : IBroker
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="Broker"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for resolving handlers.</param>
        public Broker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public TResponse Handle<TRequest, TResponse>(TRequest message) where TRequest : IMessage
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var messageHandler = _serviceProvider.GetService<IHandler<TRequest, TResponse>>();
            
            if (messageHandler == null)
            {
                throw new InvalidOperationException(
                    $"No handler registered for request type '{typeof(TRequest).Name}' " +
                    $"returning '{typeof(TResponse).Name}'. " +
                    $"Ensure the handler implements IHandler<{typeof(TRequest).Name}, {typeof(TResponse).Name}> " +
                    $"and is registered via AddCommandQuery().");
            }

            var result = messageHandler.Execute(message);
            return result;
        }

        /// <inheritdoc />
        public async Task<TResponse> HandleAsync<TRequest, TResponse>(TRequest message, CancellationToken cancellationToken = default) where TRequest : IMessage
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var messageHandler = _serviceProvider.GetService<IAsyncHandler<TRequest, TResponse>>();
            
            if (messageHandler == null)
            {
                throw new InvalidOperationException(
                    $"No handler registered for request type '{typeof(TRequest).Name}' " +
                    $"returning '{typeof(TResponse).Name}'. " +
                    $"Ensure the handler implements IAsyncHandler<{typeof(TRequest).Name}, {typeof(TResponse).Name}> " +
                    $"and is registered via AddCommandQuery().");
            }

            var result = await messageHandler.Execute(message, cancellationToken);
            return result;
        }
    }
}