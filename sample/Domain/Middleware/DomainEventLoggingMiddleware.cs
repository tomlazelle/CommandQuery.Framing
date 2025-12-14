using CommandQuery.Framing;
using GenericPipeline;
using Microsoft.Extensions.Logging;

namespace CommandQueryApiSample.Domain.Middleware;

/// <summary>
/// Example middleware that logs domain event execution.
/// Demonstrates before and after pipeline processing.
/// </summary>
/// <typeparam name="TMessage">The type of domain event message.</typeparam>
public class DomainEventLoggingMiddleware<TMessage> : IPipelineMiddleware<DomainEventContext<TMessage>>
{
    private readonly ILogger<DomainEventLoggingMiddleware<TMessage>> _logger;

    public DomainEventLoggingMiddleware(ILogger<DomainEventLoggingMiddleware<TMessage>> logger)
    {
        _logger = logger;
    }

    public async ValueTask InvokeAsync(
        DomainEventContext<TMessage> context, 
        PipelineDelegate<DomainEventContext<TMessage>> next)
    {
        var messageType = typeof(TMessage).Name;
        
        _logger.LogInformation("Before processing domain event: {MessageType}", messageType);
        
        try
        {
            await next(context);
            
            _logger.LogInformation(
                "After processing domain event: {MessageType}, Success: {Success}", 
                messageType, 
                context.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "Error processing domain event: {MessageType}", 
                messageType);
            throw;
        }
    }
}
