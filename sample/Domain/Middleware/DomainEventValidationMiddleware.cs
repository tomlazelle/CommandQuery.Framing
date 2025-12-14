using CommandQuery.Framing;
using GenericPipeline;

namespace CommandQueryApiSample.Domain.Middleware;

/// <summary>
/// Example middleware that validates domain event messages.
/// Can short-circuit the pipeline if validation fails.
/// </summary>
/// <typeparam name="TMessage">The type of domain event message.</typeparam>
public class DomainEventValidationMiddleware<TMessage> : IPipelineMiddleware<DomainEventContext<TMessage>>
{
    public async ValueTask InvokeAsync(
        DomainEventContext<TMessage> context, 
        PipelineDelegate<DomainEventContext<TMessage>> next)
    {
        // Example validation - check if message is not null
        if (context.Message == null)
        {
            context.Success = false;
            context.ErrorMessage = "Domain event message cannot be null";
            context.ShouldContinue = false;
            return;
        }

        // Additional validation logic could go here
        // For example, validate message properties using FluentValidation

        // If validation passes, continue to next middleware/handler
        await next(context);
    }
}
