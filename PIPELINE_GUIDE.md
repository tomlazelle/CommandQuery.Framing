# Domain Event Pipelines

The CommandQuery.Framing framework now supports **before and after pipelines** for domain events using the `abes.GenericPipeline` library. This allows you to add cross-cutting concerns like logging, validation, authorization, and more around your domain event handlers.

## Features

- **Middleware Pattern**: Apply middleware before and after domain event execution
- **Pipeline Composition**: Chain multiple middleware in a specific order
- **DI Integration**: Middleware can use dependency injection
- **Short-Circuit Support**: Stop pipeline execution based on conditions
- **Context Sharing**: Share data between middleware via the context

## Quick Start

### 1. Add the Package

The `abes.GenericPipeline` package is already included in CommandQuery.Framing.

### 2. Create Middleware

Create middleware by implementing `IPipelineMiddleware<DomainEventContext<TMessage>>`:

```csharp
using CommandQuery.Framing;
using GenericPipeline;

public class LoggingMiddleware<TMessage> : IPipelineMiddleware<DomainEventContext<TMessage>>
{
    private readonly ILogger _logger;

    public LoggingMiddleware(ILogger<LoggingMiddleware<TMessage>> logger)
    {
        _logger = logger;
    }

    public async ValueTask InvokeAsync(
        DomainEventContext<TMessage> context, 
        PipelineDelegate<DomainEventContext<TMessage>> next)
    {
        _logger.LogInformation("Before: {MessageType}", typeof(TMessage).Name);
        
        await next(context); // Call next middleware or handler
        
        _logger.LogInformation("After: {MessageType}, Success: {Success}", 
            typeof(TMessage).Name, 
            context.Success);
    }
}
```

### 3. Register Pipeline in Startup

Configure the pipeline for specific message types:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddCommandQuery(typeof(Startup).Assembly);
    
    // Register middleware
    services
        .AddDomainEventMiddleware<LoggingMiddleware<WidgetCreated>>()
        .AddDomainEventMiddleware<ValidationMiddleware<WidgetCreated>>();
    
    // Configure pipeline for WidgetCreated events
    services.AddDomainEventPipeline<WidgetCreated>(builder =>
    {
        builder.Use<ValidationMiddleware<WidgetCreated>>();
        builder.Use<LoggingMiddleware<WidgetCreated>>();
    });
}
```

### 4. Publish Events

Domain events will now flow through the pipeline:

```csharp
public class CreateWidget : IAsyncHandler<CreateWidgetMessage, CommandResponse<string>>
{
    private readonly IDomainEventPublisher _publisher;

    public CreateWidget(IDomainEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task<CommandResponse<string>> Execute(
        CreateWidgetMessage message,
        CancellationToken cancellationToken = default)
    {
        var widgetId = Guid.NewGuid().ToString();

        // This will execute through the configured pipeline
        await _publisher.Publish(
            new WidgetCreated { Id = widgetId, Name = message.Name }, 
            cancellationToken);

        return Response.Ok(widgetId);
    }
}
```

## DomainEventContext<TMessage>

The pipeline context provides:

```csharp
public class DomainEventContext<TMessage> : PipelineContext
{
    // The message being published
    public TMessage Message { get; set; }
    
    // Control flow - set to false to short-circuit
    public bool ShouldContinue { get; set; } = true;
    
    // Result information
    public bool Success { get; set; } = true;
    public string ErrorMessage { get; set; }
    public Exception Exception { get; set; }
    
    // From PipelineContext:
    public CancellationToken CancellationToken { get; init; }
    public IDictionary<string, object> Items { get; } // For sharing data
}
```

## Common Middleware Examples

### Validation Middleware

```csharp
public class ValidationMiddleware<TMessage> : IPipelineMiddleware<DomainEventContext<TMessage>>
{
    public async ValueTask InvokeAsync(
        DomainEventContext<TMessage> context, 
        PipelineDelegate<DomainEventContext<TMessage>> next)
    {
        if (context.Message == null)
        {
            context.Success = false;
            context.ErrorMessage = "Message cannot be null";
            context.ShouldContinue = false; // Stop the pipeline
            return;
        }

        await next(context);
    }
}
```

### Authorization Middleware

```csharp
public class AuthorizationMiddleware<TMessage> : IPipelineMiddleware<DomainEventContext<TMessage>>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationMiddleware(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async ValueTask InvokeAsync(
        DomainEventContext<TMessage> context, 
        PipelineDelegate<DomainEventContext<TMessage>> next)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        
        if (user == null || !user.Identity?.IsAuthenticated == true)
        {
            context.Success = false;
            context.ErrorMessage = "Unauthorized";
            context.ShouldContinue = false;
            return;
        }

        await next(context);
    }
}
```

### Timing Middleware

```csharp
public class TimingMiddleware<TMessage> : IPipelineMiddleware<DomainEventContext<TMessage>>
{
    private readonly ILogger _logger;

    public TimingMiddleware(ILogger<TimingMiddleware<TMessage>> logger)
    {
        _logger = logger;
    }

    public async ValueTask InvokeAsync(
        DomainEventContext<TMessage> context, 
        PipelineDelegate<DomainEventContext<TMessage>> next)
    {
        var sw = Stopwatch.StartNew();
        
        try
        {
            await next(context);
        }
        finally
        {
            sw.Stop();
            _logger.LogInformation(
                "Domain event {MessageType} processed in {ElapsedMs}ms",
                typeof(TMessage).Name,
                sw.ElapsedMilliseconds);
        }
    }
}
```

## Middleware Ordering

You can control middleware execution order:

```csharp
// Using IOrderedMiddleware for coarse ordering
public class EarlyMiddleware : IPipelineMiddleware<MyContext>, IOrderedMiddleware
{
    public int Order => -10; // Lower runs first
    
    public async ValueTask InvokeAsync(MyContext context, PipelineDelegate<MyContext> next)
    {
        await next(context);
    }
}

// Using IRunBefore<T> constraint
public class ValidationMiddleware : 
    IPipelineMiddleware<MyContext>, 
    IRunBefore<LoggingMiddleware> // Runs before logging
{
    public async ValueTask InvokeAsync(MyContext context, PipelineDelegate<MyContext> next)
    {
        await next(context);
    }
}

// Using IRunAfter<T> constraint
public class CleanupMiddleware : 
    IPipelineMiddleware<MyContext>, 
    IRunAfter<LoggingMiddleware> // Runs after logging
{
    public async ValueTask InvokeAsync(MyContext context, PipelineDelegate<MyContext> next)
    {
        await next(context);
    }
}
```

## Short-Circuiting

Stop the pipeline early by setting `ShouldContinue = false`:

```csharp
public async ValueTask InvokeAsync(
    DomainEventContext<TMessage> context, 
    PipelineDelegate<TMessage> next)
{
    if (SomeCondition())
    {
        context.ShouldContinue = false;
        context.ErrorMessage = "Condition not met";
        return; // Don't call next()
    }

    await next(context);
}
```

## Benefits

✅ **Separation of Concerns**: Keep cross-cutting logic separate from business logic  
✅ **Reusability**: Write middleware once, use across multiple event types  
✅ **Testability**: Test middleware independently  
✅ **Flexibility**: Enable/disable middleware via configuration  
✅ **Performance Monitoring**: Add timing and metrics easily  
✅ **Error Handling**: Centralized exception handling  

## See Also

- [abes.GenericPipeline Documentation](https://github.com/tomlazelle/pipeline)
- [Sample Implementation](sample/Domain/Middleware/)
- [DomainEventContext API](src/CommandQuery.Framing/DomainEventContext.cs)
