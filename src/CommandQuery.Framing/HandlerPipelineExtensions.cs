using System;
using GenericPipeline;
using Microsoft.Extensions.DependencyInjection;

namespace CommandQuery.Framing;

/// <summary>
/// Extension methods for configuring handler pipelines with middleware for both synchronous and asynchronous handlers.
/// </summary>
public static class HandlerPipelineExtensions
{
    /// <summary>
    /// Adds a synchronous handler pipeline with middleware for a specific request and response type.
    /// Uses <see cref="SyncPipelineBuilder{TContext}"/> for pure synchronous execution without async overhead.
    /// </summary>
    /// <typeparam name="TRequest">The type of request this pipeline handles.</typeparam>
    /// <typeparam name="TResponse">The type of response this pipeline returns.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configurePipeline">Action to configure the pipeline with middleware.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddHandlerPipeline&lt;CreateWidget, CommandResponse&gt;(builder =&gt;
    /// {
    ///     builder.Use&lt;ValidationMiddleware&lt;CreateWidget, CommandResponse&gt;&gt;();
    ///     builder.Use&lt;LoggingMiddleware&lt;CreateWidget, CommandResponse&gt;&gt;();
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddHandlerPipeline<TRequest, TResponse>(
        this IServiceCollection services,
        Action<SyncPipelineBuilder<HandlerContext<TRequest, TResponse>>> configurePipeline)
        where TRequest : IMessage
    {
        services.AddSingleton<SyncPipelineDelegate<HandlerContext<TRequest, TResponse>>>(sp =>
        {
            var builder = new SyncPipelineBuilder<HandlerContext<TRequest, TResponse>>();
            configurePipeline(builder);
            
            var diagnostics = sp.GetService<ISyncPipelineDiagnostics<HandlerContext<TRequest, TResponse>>>();
            return builder.Build(diagnostics);
        });

        return services;
    }

    /// <summary>
    /// Adds an asynchronous handler pipeline with middleware for a specific request and response type.
    /// Uses <see cref="PipelineBuilder{TContext}"/> for optimal async/await performance with ValueTask.
    /// </summary>
    /// <typeparam name="TRequest">The type of request this pipeline handles.</typeparam>
    /// <typeparam name="TResponse">The type of response this pipeline returns.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configurePipeline">Action to configure the pipeline with middleware.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddAsyncHandlerPipeline&lt;GetWidget, Widget&gt;(builder =&gt;
    /// {
    ///     builder.Use&lt;CachingMiddleware&lt;GetWidget, Widget&gt;&gt;();
    ///     builder.Use&lt;LoggingMiddleware&lt;GetWidget, Widget&gt;&gt;();
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddAsyncHandlerPipeline<TRequest, TResponse>(
        this IServiceCollection services,
        Action<PipelineBuilder<AsyncHandlerContext<TRequest, TResponse>>> configurePipeline)
        where TRequest : IMessage
    {
        services.AddSingleton<PipelineDelegate<AsyncHandlerContext<TRequest, TResponse>>>(sp =>
        {
            var builder = new PipelineBuilder<AsyncHandlerContext<TRequest, TResponse>>();
            configurePipeline(builder);
            
            var diagnostics = sp.GetService<IPipelineDiagnostics<AsyncHandlerContext<TRequest, TResponse>>>();
            return builder.Build(diagnostics);
        });

        return services;
    }

    /// <summary>
    /// Adds a scoped middleware type for use in handler pipelines.
    /// The middleware will be resolved from DI for each pipeline execution.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddHandlerMiddleware&lt;LoggingMiddleware&lt;CreateWidget, CommandResponse&gt;&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection AddHandlerMiddleware<TMiddleware>(
        this IServiceCollection services)
        where TMiddleware : class
    {
        services.AddScoped<TMiddleware>();
        return services;
    }
}
