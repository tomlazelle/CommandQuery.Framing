using System;
using System.Threading;
using GenericPipeline;

namespace CommandQuery.Framing;

/// <summary>
/// Context for synchronous handler pipeline execution.
/// Contains the request and response for the handler, and allows middleware to access and modify execution state.
/// </summary>
/// <typeparam name="TRequest">The type of request message.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public class HandlerContext<TRequest, TResponse> : PipelineContext
    where TRequest : IMessage
{
    /// <summary>
    /// Gets or sets the request message being processed.
    /// </summary>
    public TRequest Request { get; set; }

    /// <summary>
    /// Gets or sets the response from the handler.
    /// Middleware can access or modify this after handler execution.
    /// </summary>
    public TResponse Response { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether handler processing should continue.
    /// Set to false in middleware to short-circuit the pipeline and skip handler execution.
    /// </summary>
    public bool ShouldContinue { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the handler was processed successfully.
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Gets or sets an error message if processing failed.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets an exception that occurred during processing.
    /// </summary>
    public Exception Exception { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HandlerContext{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="request">The request message to process.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    public HandlerContext(TRequest request, CancellationToken cancellationToken = default)
    {
        Request = request;
        CancellationToken = cancellationToken;
    }
}
