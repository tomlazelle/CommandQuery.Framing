using CommandQuery.Framing;
using GenericPipeline;
using Microsoft.Extensions.Logging;

namespace CommandQueryApiSample.Domain.Middleware;

/// <summary>
/// Example middleware that logs synchronous handler execution.
/// Demonstrates before and after pipeline processing for sync handlers.
/// </summary>
/// <typeparam name="TRequest">The type of request message.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public class HandlerLoggingMiddleware<TRequest, TResponse> : ISyncPipelineMiddleware<HandlerContext<TRequest, TResponse>>
    where TRequest : IMessage
{
    private readonly ILogger<HandlerLoggingMiddleware<TRequest, TResponse>> _logger;

    public HandlerLoggingMiddleware(ILogger<HandlerLoggingMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public void Invoke(
        HandlerContext<TRequest, TResponse> context, 
        Action next)
    {
        var requestType = typeof(TRequest).Name;
        var responseType = typeof(TResponse).Name;
        
        _logger.LogInformation(
            "Before sync handler execution: {RequestType} -> {ResponseType}", 
            requestType, 
            responseType);
        
        try
        {
            next();
            
            _logger.LogInformation(
                "After sync handler execution: {RequestType} -> {ResponseType}, Success: {Success}", 
                requestType, 
                responseType,
                context.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "Error in sync handler: {RequestType} -> {ResponseType}", 
                requestType, 
                responseType);
            throw;
        }
    }
}
