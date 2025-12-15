using System;
using System.Threading;
using System.Threading.Tasks;
using GenericPipeline;
using Microsoft.Extensions.DependencyInjection;

namespace CommandQuery.Framing
{
    /// <summary>
    /// Central broker for dispatching commands and queries to their registered handlers.
    /// Supports pipeline middleware for both synchronous and asynchronous handlers.
    /// </summary>
    public class Broker : IBroker
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Broker"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for resolving handlers.</param>
        /// <param name="scopeFactory">The scope factory for creating DI scopes for pipeline execution.</param>
        public Broker(IServiceProvider serviceProvider, IServiceScopeFactory scopeFactory)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        /// <inheritdoc />
        public TResponse Handle<TRequest, TResponse>(TRequest message) where TRequest : IMessage
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var context = new HandlerContext<TRequest, TResponse>(message);

            // Execute synchronous pipeline if configured
            var pipeline = _serviceProvider.GetService<SyncPipelineDelegate<HandlerContext<TRequest, TResponse>>>();
            if (pipeline != null)
            {
                using var scope = _scopeFactory.CreateScope();
                context.Items["__GenericPipeline_ServiceProvider"] = scope.ServiceProvider;
                pipeline(context);

                // Check if pipeline indicated we should stop
                if (!context.ShouldContinue)
                {
                    return context.Response;
                }
            }

            // Execute handler
            var messageHandler = _serviceProvider.GetService<IHandler<TRequest, TResponse>>();
            
            if (messageHandler == null)
            {
                throw new InvalidOperationException(
                    $"No handler registered for request type '{typeof(TRequest).Name}' " +
                    $"returning '{typeof(TResponse).Name}'. " +
                    $"Ensure the handler implements IHandler<{typeof(TRequest).Name}, {typeof(TResponse).Name}> " +
                    $"and is registered via AddCommandQuery().");
            }

            try
            {
                var result = messageHandler.Execute(message);
                context.Response = result;
                context.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                context.Success = false;
                context.Exception = ex;
                context.ErrorMessage = ex.Message;
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<TResponse> HandleAsync<TRequest, TResponse>(TRequest message, CancellationToken cancellationToken = default) where TRequest : IMessage
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var context = new AsyncHandlerContext<TRequest, TResponse>(message, cancellationToken);

            // Execute asynchronous pipeline if configured
            var pipeline = _serviceProvider.GetService<PipelineDelegate<AsyncHandlerContext<TRequest, TResponse>>>();
            if (pipeline != null)
            {
                using var scope = _scopeFactory.CreateScope();
                context.Items["__GenericPipeline_ServiceProvider"] = scope.ServiceProvider;
                await pipeline(context);

                // Check if pipeline indicated we should stop
                if (!context.ShouldContinue)
                {
                    return context.Response;
                }
            }

            // Execute handler
            var messageHandler = _serviceProvider.GetService<IAsyncHandler<TRequest, TResponse>>();
            
            if (messageHandler == null)
            {
                throw new InvalidOperationException(
                    $"No handler registered for request type '{typeof(TRequest).Name}' " +
                    $"returning '{typeof(TResponse).Name}'. " +
                    $"Ensure the handler implements IAsyncHandler<{typeof(TRequest).Name}, {typeof(TResponse).Name}> " +
                    $"and is registered via AddCommandQuery().");
            }

            try
            {
                var result = await messageHandler.Execute(message, cancellationToken);
                context.Response = result;
                context.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                context.Success = false;
                context.Exception = ex;
                context.ErrorMessage = ex.Message;
                throw;
            }
        }
    }
}