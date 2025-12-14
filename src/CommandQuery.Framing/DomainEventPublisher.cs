using System;
using System.Threading;
using System.Threading.Tasks;
using GenericPipeline;
using Microsoft.Extensions.DependencyInjection;

namespace CommandQuery.Framing
{
    /// <summary>
    /// Publisher for domain events with support for pipeline middleware.
    /// Executes before and after pipelines around domain event handlers.
    /// </summary>
    public class DomainEventPublisher : IDomainEventPublisher
    {
        public event EventHandler MessageSent;
        public event EventHandler<DomainEventArgs> MessageResult;

        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEventPublisher"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
        /// <param name="scopeFactory">The scope factory for creating DI scopes for pipeline execution.</param>
        public DomainEventPublisher(IServiceProvider serviceProvider, IServiceScopeFactory scopeFactory)
        {
            _serviceProvider = serviceProvider;
            _scopeFactory = scopeFactory;
        }

        /// <inheritdoc />
        public async Task Publish<TMessageType>(TMessageType message, CancellationToken cancellationToken = default)
        {
            var context = new DomainEventContext<TMessageType>(message, cancellationToken);

            // Execute before pipeline if configured
            var beforePipeline = _serviceProvider.GetService<PipelineDelegate<DomainEventContext<TMessageType>>>();
            if (beforePipeline != null)
            {
                using var scope = _scopeFactory.CreateScope();
                context.Items["__GenericPipeline_ServiceProvider"] = scope.ServiceProvider;
                await beforePipeline(context);

                // Check if pipeline indicated we should stop
                if (!context.ShouldContinue)
                {
                    return;
                }
            }

            // Execute domain event handlers
            var events = _serviceProvider.GetServices<IDomainEvent<TMessageType>>();

            foreach (var domainEvent in events)
            {
                if (!context.ShouldContinue)
                    break;

                domainEvent.OnComplete += DomainEvent_OnComplete;
                
                try
                {
                    MessageSent?.Invoke(this, new DomainEventArgs());
                    await domainEvent.Execute(message);
                }
                catch (Exception ex)
                {
                    context.Success = false;
                    context.Exception = ex;
                    context.ErrorMessage = ex.Message;
                    throw;
                }
                finally
                {
                    domainEvent.OnComplete -= DomainEvent_OnComplete;
                }
            }

            // Execute after pipeline if configured
            var afterPipeline = _serviceProvider.GetService<PipelineDelegate<DomainEventContext<TMessageType>>>();
            if (afterPipeline != null && beforePipeline != null) // Only run after if before was configured
            {
                using var scope = _scopeFactory.CreateScope();
                context.Items["__GenericPipeline_ServiceProvider"] = scope.ServiceProvider;
                await afterPipeline(context);
            }
        }

        private void DomainEvent_OnComplete(object sender, DomainEventArgs e)
        {
            MessageResult?.Invoke(this, e);
        }
    }
}