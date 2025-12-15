using CommandQuery.Framing;
using GenericPipeline;
using Microsoft.Extensions.Logging;

namespace CommandQueryApiSample.Domain.Middleware;

/// <summary>
/// Example middleware that logs async handler execution.
/// Demonstrates before and after pipeline processing for async handlers.
/// </summary>
/// <typeparam name="TRequest">The type of request message.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public class AsyncHandlerLoggingMiddleware<TRequest, TResponse> : IPipelineMiddleware<AsyncHandlerContext<TRequest, TResponse>>
    where TRequest : IMessage
{
    private readonly ILogger<AsyncHandlerLoggingMiddleware<TRequest, TResponse>> _logger;

    public AsyncHandlerLoggingMiddleware(ILogger<AsyncHandlerLoggingMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async ValueTask InvokeAsync(
        AsyncHandlerContext<TRequest, TResponse> context, 
        PipelineDelegate<AsyncHandlerContext<TRequest, TResponse>> next)
    {
        var requestType = typeof(TRequest).Name;
        var responseType = typeof(TResponse).Name;
        
        _logger.LogInformation(
            "Before async handler execution: {RequestType} -> {ResponseType}", 
            requestType, 
            responseType);
        
        try
        {
            await next(context);
            
            _logger.LogInformation(
                "After async handler execution: {RequestType} -> {ResponseType}, Success: {Success}", 
                requestType, 
                responseType,
                context.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "Error in async handler: {RequestType} -> {ResponseType}", 
                requestType, 
                responseType);
            throw;
        }
    }
}
