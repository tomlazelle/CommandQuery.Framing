using System;
using System.Threading;
using GenericPipeline;

namespace CommandQuery.Framing;

/// <summary>
/// Context for domain event pipeline execution.
/// Contains the message and allows middleware to access and modify execution state.
/// </summary>
/// <typeparam name="TMessage">The type of message being published.</typeparam>
public class DomainEventContext<TMessage> : PipelineContext
{
    /// <summary>
    /// Gets or sets the message being published to domain event handlers.
    /// </summary>
    public TMessage Message { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the domain event processing should continue.
    /// Set to false in middleware to short-circuit the pipeline.
    /// </summary>
    public bool ShouldContinue { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the domain event was processed successfully.
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
    /// Initializes a new instance of the <see cref="DomainEventContext{TMessage}"/> class.
    /// </summary>
    /// <param name="message">The message to publish.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    public DomainEventContext(TMessage message, CancellationToken cancellationToken = default)
    {
        Message = message;
        CancellationToken = cancellationToken;
    }
}
