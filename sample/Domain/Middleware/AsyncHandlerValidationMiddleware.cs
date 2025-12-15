using CommandQuery.Framing;
using GenericPipeline;

namespace CommandQueryApiSample.Domain.Middleware;

/// <summary>
/// Example middleware that validates async handler request messages.
/// Can short-circuit the pipeline if validation fails.
/// </summary>
/// <typeparam name="TRequest">The type of request message.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public class AsyncHandlerValidationMiddleware<TRequest, TResponse> : IPipelineMiddleware<AsyncHandlerContext<TRequest, TResponse>>
    where TRequest : IMessage
{
    public async ValueTask InvokeAsync(
        AsyncHandlerContext<TRequest, TResponse> context, 
        PipelineDelegate<AsyncHandlerContext<TRequest, TResponse>> next)
    {
        // Example validation - check if request is not null
        if (context.Request == null)
        {
            context.Success = false;
            context.ErrorMessage = "Request message cannot be null";
            context.ShouldContinue = false;
            
            // For CommandResponse, create an error response
            if (typeof(TResponse) == typeof(ICommandResponse))
            {
                context.Response = (TResponse)(object)new CommandResponse 
                { 
                    Success = false, 
                    Message = context.ErrorMessage 
                };
            }
            
            return;
        }

        // Additional validation logic could go here
        // For example, validate request properties using FluentValidation

        // If validation passes, continue to next middleware/handler
        await next(context);
    }
}
