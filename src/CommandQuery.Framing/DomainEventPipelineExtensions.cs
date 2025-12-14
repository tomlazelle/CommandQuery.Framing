using System;
using GenericPipeline;
using Microsoft.Extensions.DependencyInjection;

namespace CommandQuery.Framing;

/// <summary>
/// Extension methods for configuring domain event pipelines with before and after middleware.
/// </summary>
public static class DomainEventPipelineExtensions
{
    /// <summary>
    /// Adds a domain event pipeline with before and after middleware for a specific message type.
    /// </summary>
    /// <typeparam name="TMessage">The type of message this pipeline handles.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configurePipeline">Action to configure the pipeline with middleware.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddDomainEventPipeline&lt;WidgetCreated&gt;(builder =&gt;
    /// {
    ///     builder.Use&lt;LoggingMiddleware&lt;DomainEventContext&lt;WidgetCreated&gt;&gt;&gt;();
    ///     builder.Use&lt;ValidationMiddleware&lt;DomainEventContext&lt;WidgetCreated&gt;&gt;&gt;();
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddDomainEventPipeline<TMessage>(
        this IServiceCollection services,
        Action<PipelineBuilder<DomainEventContext<TMessage>>> configurePipeline)
    {
        services.AddSingleton<PipelineDelegate<DomainEventContext<TMessage>>>(sp =>
        {
            var builder = new PipelineBuilder<DomainEventContext<TMessage>>();
            configurePipeline(builder);
            
            var diagnostics = sp.GetService<IPipelineDiagnostics<DomainEventContext<TMessage>>>();
            return builder.Build(diagnostics);
        });

        return services;
    }

    /// <summary>
    /// Adds a scoped middleware type for use in domain event pipelines.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDomainEventMiddleware<TMiddleware>(
        this IServiceCollection services)
        where TMiddleware : class
    {
        services.AddScoped<TMiddleware>();
        return services;
    }
}
