# Handler Pipelines Guide

CommandQuery.Framing supports **before and after pipelines** for command and query handlers using the `abes.GenericPipeline` library. This allows you to add cross-cutting concerns like logging, validation, caching, authorization, and more around your handler execution.

## Features

- **Separate Sync and Async Pipelines**: Pure synchronous and asynchronous implementations without mixing concerns
- **Middleware Pattern**: Apply middleware before and after handler execution
- **Pipeline Composition**: Chain multiple middleware in a specific order
- **DI Integration**: Middleware can use dependency injection
- **Short-Circuit Support**: Stop pipeline execution based on conditions
- **Context Sharing**: Share data between middleware via the context

## Architecture

The framework provides **two independent pipeline implementations**:

### Synchronous Handler Pipeline
- Uses `SyncPipelineBuilder<HandlerContext<TRequest, TResponse>>`
- Middleware implements `ISyncPipelineMiddleware<HandlerContext<TRequest, TResponse>>`
- Pure synchronous execution (no `ValueTask` overhead)
- Executed before/after `IHandler<TRequest, TResponse>`

### Asynchronous Handler Pipeline
- Uses `PipelineBuilder<AsyncHandlerContext<TRequest, TResponse>>`
- Middleware implements `IPipelineMiddleware<AsyncHandlerContext<TRequest, TResponse>>`
- Uses `ValueTask` for optimal async performance
- Executed before/after `IAsyncHandler<TRequest, TResponse>`

**Important**: The two pipelines cannot be mixed. Choose sync or async based on your handler type.

## Quick Start

### 1. Create Middleware

#### Async Handler Middleware

```csharp
using CommandQuery.Framing;
using GenericPipeline;
using Microsoft.Extensions.Logging;

public class LoggingMiddleware<TRequest, TResponse> 
    : IPipelineMiddleware<AsyncHandlerContext<TRequest, TResponse>>
    where TRequest : IMessage
{
    private readonly ILogger _logger;

    public LoggingMiddleware(ILogger<LoggingMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async ValueTask InvokeAsync(
        AsyncHandlerContext<TRequest, TResponse> context, 
        PipelineDelegate<AsyncHandlerContext<TRequest, TResponse>> next)
    {
        _logger.LogInformation("Before: {RequestType}", typeof(TRequest).Name);
        
        await next(context); // Call next middleware or handler
        
        _logger.LogInformation("After: {RequestType}, Success: {Success}", 
            typeof(TRequest).Name, 
            context.Success);
    }
}
```

#### Sync Handler Middleware

```csharp
using CommandQuery.Framing;
using GenericPipeline;
using Microsoft.Extensions.Logging;

public class LoggingMiddleware<TRequest, TResponse> 
    : ISyncPipelineMiddleware<HandlerContext<TRequest, TResponse>>
    where TRequest : IMessage
{
    private readonly ILogger _logger;

    public LoggingMiddleware(ILogger<LoggingMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public void Invoke(
        HandlerContext<TRequest, TResponse> context, 
        Action next)
    {
        _logger.LogInformation("Before: {RequestType}", typeof(TRequest).Name);
        
        next(); // Call next middleware or handler
        
        _logger.LogInformation("After: {RequestType}, Success: {Success}", 
            typeof(TRequest).Name, 
            context.Success);
    }
}
```

### 2. Register Pipeline

#### For Async Handlers

```csharp
services
    .AddHandlerMiddleware<LoggingMiddleware<GetWidget, Widget>>()
    .AddHandlerMiddleware<CachingMiddleware<GetWidget, Widget>>()
    .AddAsyncHandlerPipeline<GetWidget, Widget>(builder =>
    {
        builder.Use<CachingMiddleware<GetWidget, Widget>>();
        builder.Use<LoggingMiddleware<GetWidget, Widget>>();
    });
```

#### For Sync Handlers

```csharp
services
    .AddHandlerMiddleware<LoggingMiddleware<CreateWidget, CommandResponse>>()
    .AddHandlerMiddleware<ValidationMiddleware<CreateWidget, CommandResponse>>()
    .AddHandlerPipeline<CreateWidget, CommandResponse>(builder =>
    {
        builder.Use<ValidationMiddleware<CreateWidget, CommandResponse>>();
        builder.Use<LoggingMiddleware<CreateWidget, CommandResponse>>();
    });
```

### 3. Execute Handlers

No changes needed! The broker automatically executes pipelines:

```csharp
// Async handler with pipeline
var widget = await broker.HandleAsync<GetWidget, Widget>(
    new GetWidget { Id = 123 }, 
    cancellationToken);

// Sync handler with pipeline
var response = broker.Handle<CreateWidget, CommandResponse>(
    new CreateWidget { Name = "NewWidget" });
```

## Handler Context

### AsyncHandlerContext<TRequest, TResponse>

Properties available to middleware:

```csharp
public class AsyncHandlerContext<TRequest, TResponse>
{
    public TRequest Request { get; set; }              // The request message
    public TResponse Response { get; set; }            // The response (available after handler)
    public bool ShouldContinue { get; set; } = true;   // Set to false to short-circuit
    public bool Success { get; set; } = true;          // Indicates success/failure
    public string ErrorMessage { get; set; }           // Error message if failed
    public Exception Exception { get; set; }           // Exception if occurred
    public CancellationToken CancellationToken { get; }
    public IDictionary<string, object> Items { get; }  // Share data between middleware
}
```

### HandlerContext<TRequest, TResponse>

Same properties as `AsyncHandlerContext` but for synchronous pipelines.

## Common Middleware Patterns

### Validation Middleware

```csharp
public class ValidationMiddleware<TRequest, TResponse> 
    : IPipelineMiddleware<AsyncHandlerContext<TRequest, TResponse>>
    where TRequest : IMessage
{
    private readonly IValidator<TRequest> _validator;

    public ValidationMiddleware(IValidator<TRequest> validator)
    {
        _validator = validator;
    }

    public async ValueTask InvokeAsync(
        AsyncHandlerContext<TRequest, TResponse> context, 
        PipelineDelegate<AsyncHandlerContext<TRequest, TResponse>> next)
    {
        var validationResult = await _validator.ValidateAsync(context.Request);
        
        if (!validationResult.IsValid)
        {
            context.Success = false;
            context.ErrorMessage = string.Join("; ", 
                validationResult.Errors.Select(e => e.ErrorMessage));
            context.ShouldContinue = false;
            
            // Optionally create an error response
            if (typeof(TResponse) == typeof(CommandResponse))
            {
                context.Response = (TResponse)(object)new CommandResponse 
                { 
                    Success = false, 
                    Message = context.ErrorMessage 
                };
            }
            
            return; // Short-circuit
        }

        await next(context);
    }
}
```

### Caching Middleware

```csharp
public class CachingMiddleware<TRequest, TResponse> 
    : IPipelineMiddleware<AsyncHandlerContext<TRequest, TResponse>>
    where TRequest : IMessage
{
    private readonly IDistributedCache _cache;
    private readonly ILogger _logger;

    public CachingMiddleware(
        IDistributedCache cache,
        ILogger<CachingMiddleware<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async ValueTask InvokeAsync(
        AsyncHandlerContext<TRequest, TResponse> context, 
        PipelineDelegate<AsyncHandlerContext<TRequest, TResponse>> next)
    {
        var cacheKey = $"{typeof(TRequest).Name}:{JsonSerializer.Serialize(context.Request)}";
        
        // Try to get from cache
        var cachedValue = await _cache.GetStringAsync(cacheKey);
        if (cachedValue != null)
        {
            _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
            context.Response = JsonSerializer.Deserialize<TResponse>(cachedValue);
            context.Success = true;
            return; // Short-circuit, don't call handler
        }

        // Execute handler
        await next(context);

        // Cache the response if successful
        if (context.Success && context.Response != null)
        {
            await _cache.SetStringAsync(
                cacheKey, 
                JsonSerializer.Serialize(context.Response),
                new DistributedCacheEntryOptions 
                { 
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) 
                });
            
            _logger.LogInformation("Cached result for {CacheKey}", cacheKey);
        }
    }
}
```

### Performance Monitoring Middleware

```csharp
public class PerformanceMiddleware<TRequest, TResponse> 
    : IPipelineMiddleware<AsyncHandlerContext<TRequest, TResponse>>
    where TRequest : IMessage
{
    private readonly ILogger _logger;

    public PerformanceMiddleware(ILogger<PerformanceMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async ValueTask InvokeAsync(
        AsyncHandlerContext<TRequest, TResponse> context, 
        PipelineDelegate<AsyncHandlerContext<TRequest, TResponse>> next)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "Handler {RequestType} took {ElapsedMilliseconds}ms",
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
```

### Authorization Middleware

```csharp
public class AuthorizationMiddleware<TRequest, TResponse> 
    : IPipelineMiddleware<AsyncHandlerContext<TRequest, TResponse>>
    where TRequest : IMessage
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public AuthorizationMiddleware(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    public async ValueTask InvokeAsync(
        AsyncHandlerContext<TRequest, TResponse> context, 
        PipelineDelegate<AsyncHandlerContext<TRequest, TResponse>> next)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        
        if (user == null || !user.Identity?.IsAuthenticated == true)
        {
            context.Success = false;
            context.ErrorMessage = "User is not authenticated";
            context.ShouldContinue = false;
            
            if (typeof(TResponse) == typeof(CommandResponse))
            {
                context.Response = (TResponse)(object)new CommandResponse 
                { 
                    Success = false, 
                    Message = "Unauthorized" 
                };
            }
            
            return;
        }

        // Additional authorization checks...
        var authResult = await _authorizationService.AuthorizeAsync(
            user, 
            context.Request, 
            "HandlerPolicy");

        if (!authResult.Succeeded)
        {
            context.Success = false;
            context.ErrorMessage = "User is not authorized";
            context.ShouldContinue = false;
            return;
        }

        await next(context);
    }
}
```

## Middleware Ordering

Control execution order using:

- **`IOrderedMiddleware`**: Set explicit `Order` property (lower runs first)
- **`IRunBefore<TMiddleware>`**: Ensures this middleware runs before `TMiddleware`
- **`IRunAfter<TMiddleware>`**: Ensures this middleware runs after `TMiddleware`

### Example

```csharp
public class EarlyMiddleware<TRequest, TResponse> 
    : IPipelineMiddleware<AsyncHandlerContext<TRequest, TResponse>>, 
      IOrderedMiddleware
    where TRequest : IMessage
{
    public int Order => -100; // Runs early

    public ValueTask InvokeAsync(
        AsyncHandlerContext<TRequest, TResponse> context,
        PipelineDelegate<AsyncHandlerContext<TRequest, TResponse>> next)
    {
        return next(context);
    }
}

public class ValidationMiddleware<TRequest, TResponse> 
    : IPipelineMiddleware<AsyncHandlerContext<TRequest, TResponse>>,
      IRunBefore<LoggingMiddleware<TRequest, TResponse>>
    where TRequest : IMessage
{
    // Ensures validation runs before logging
    public ValueTask InvokeAsync(
        AsyncHandlerContext<TRequest, TResponse> context,
        PipelineDelegate<AsyncHandlerContext<TRequest, TResponse>> next)
    {
        // Validation logic...
        return next(context);
    }
}
```

## Complete Example

```csharp
// Startup.cs or Program.cs
public void ConfigureServices(IServiceCollection services)
{
    // Register handlers
    services.AddCommandQuery(typeof(Program).Assembly);
    
    // Register middleware
    services.AddHandlerMiddleware<PerformanceMiddleware<GetWidget, Widget>>();
    services.AddHandlerMiddleware<CachingMiddleware<GetWidget, Widget>>();
    services.AddHandlerMiddleware<LoggingMiddleware<GetWidget, Widget>>();
    
    // Configure async pipeline for GetWidget
    services.AddAsyncHandlerPipeline<GetWidget, Widget>(builder =>
    {
        builder.Use<PerformanceMiddleware<GetWidget, Widget>>();
        builder.Use<CachingMiddleware<GetWidget, Widget>>();
        builder.Use<LoggingMiddleware<GetWidget, Widget>>();
    });
    
    // Register sync middleware
    services.AddHandlerMiddleware<ValidationMiddleware<CreateWidget, CommandResponse>>();
    services.AddHandlerMiddleware<LoggingMiddleware<CreateWidget, CommandResponse>>();
    
    // Configure sync pipeline for CreateWidget
    services.AddHandlerPipeline<CreateWidget, CommandResponse>(builder =>
    {
        builder.Use<ValidationMiddleware<CreateWidget, CommandResponse>>();
        builder.Use<LoggingMiddleware<CreateWidget, CommandResponse>>();
    });
}
```

## Benefits of Handler Pipelines

1. **Cross-Cutting Concerns**: Centralize logging, validation, caching, etc.
2. **Request/Response Transformation**: Modify requests before handlers, responses after
3. **Short-Circuiting**: Skip handler execution (e.g., cache hits, validation failures)
4. **Error Handling**: Consistent exception handling across all handlers
5. **Performance Monitoring**: Measure handler execution time
6. **Authorization**: Check permissions before executing handlers
7. **Retry Logic**: Automatically retry failed operations
8. **Consistent Behavior**: Apply same cross-cutting concerns to many handlers

## Comparison with DomainEvent Pipelines

| Feature | Handler Pipelines | DomainEvent Pipelines |
|---------|------------------|----------------------|
| When Executed | Before/after handler execution | Before/after domain event handlers |
| Use Case | Commands & Queries | Domain events / side effects |
| Request/Response | Has typed request and response | Single message only |
| Return Value | Handler returns value to caller | No return value expected |
| Short-Circuit | Can return response early | Can stop event propagation |

## Related Documentation

- [abes.GenericPipeline Documentation](https://github.com/tomlazelle/pipeline)
- [Domain Event Pipelines](PIPELINE_GUIDE.md)
- [Handler Requirements](HANDLER_PIPELINE_REQUIREMENTS.md)

## Troubleshooting

### Pipeline Not Executing

- Ensure you've registered the pipeline using `AddHandlerPipeline` or `AddAsyncHandlerPipeline`
- Ensure middleware is registered with `AddHandlerMiddleware`
- Check that you're using the correct pipeline type (sync vs async) for your handler

### Middleware Not Running in Expected Order

- Check for `IOrderedMiddleware`, `IRunBefore<T>`, or `IRunAfter<T>` implementations
- Remember that middleware registered later executes first (reverse order) unless constraints are specified

### Handler Not Found Error

- Pipeline execution happens before handler resolution
- If pipeline short-circuits, handler may not be called
- Ensure handler is registered with `AddCommandQuery()`
