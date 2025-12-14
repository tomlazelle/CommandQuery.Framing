# CommandQuery.Framing

**Current Version:** 1.0.11  
**Target Framework:** .NET 9.0

A lightweight, extensible CQRS (Command Query Responsibility Segregation) framework for .NET that simplifies command, query, and domain event handling with built-in pipeline support.

## Features

✨ **Simple API** - Single `IBroker` interface for executing commands and queries  
📦 **Auto-Registration** - Automatic handler discovery via assembly scanning  
🔄 **Domain Events** - Built-in publisher/subscriber pattern with pipeline middleware  
🎯 **Type-Safe** - Strongly-typed requests and responses  
⚡ **Async First** - Full async/await support with `CancellationToken`  
🔌 **Pipeline Middleware** - Add cross-cutting concerns (logging, validation, etc.) to domain events  
📝 **Well-Documented** - Comprehensive XML documentation for IntelliSense  
🧪 **Tested** - Includes test suite and working sample application

## Installation

```bash
dotnet add package CommandQuery.Framing
```

## Quick Start

### Setup

Register CommandQuery services in your `Startup.cs` or `Program.cs`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Automatically discovers and registers all handlers in the assembly
    services.AddCommandQuery(typeof(Startup).Assembly);
}
```

### Using the Broker

Inject `IBroker` into your controllers or services to execute commands and queries:

```csharp
[ApiController]
public class WidgetController : ControllerBase
{
    private readonly IBroker _broker;

    public WidgetController(IBroker broker)
    {
        _broker = broker;
    }

    [HttpPost("widget")]
    public async Task<IActionResult> CreateWidget(
        [FromBody] CreateWidgetMessage request, 
        CancellationToken cancellationToken)
    {
        var result = await _broker.HandleAsync<CreateWidgetMessage, CommandResponse<string>>(
            request, 
            cancellationToken);

        return result.Success 
            ? Ok(result.Data) 
            : BadRequest(result.Message);
    }

    [HttpGet("widget/{id}")]
    public async Task<IActionResult> GetWidget(
        string id, 
        CancellationToken cancellationToken)
    {
        var widget = await _broker.HandleAsync<GetWidget, Widget>(
            new GetWidget { Id = id }, 
            cancellationToken);

        return Ok(widget);
    }
}
```

### Creating Handlers

#### Command Handler

Implement `IAsyncHandler<TRequest, TResponse>` for commands that modify state:

```csharp
public class CreateWidgetHandler : IAsyncHandler<CreateWidgetMessage, CommandResponse<string>>
{
    private readonly IDomainEventPublisher _publisher;
    private readonly IWidgetRepository _repository;

    public CreateWidgetHandler(
        IDomainEventPublisher publisher,
        IWidgetRepository repository)
    {
        _publisher = publisher;
        _repository = repository;
    }
    
    public async Task<CommandResponse<string>> Execute(
        CreateWidgetMessage message, 
        CancellationToken cancellationToken = default)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(message?.Name))
            return Response.Failed<string>("Widget name is required");

        // Create widget
        var widgetId = Guid.NewGuid().ToString();
        await _repository.CreateAsync(widgetId, message.Name, cancellationToken);

        // Publish domain event
        await _publisher.Publish(
            new WidgetCreated { Id = widgetId, Name = message.Name }, 
            cancellationToken);

        return Response.Ok(widgetId);
    }
}
```

#### Query Handler

Implement `IAsyncHandler<TRequest, TResponse>` for queries that retrieve data:

```csharp
public class GetWidgetQuery : IAsyncHandler<GetWidget, Widget>
{
    private readonly IWidgetRepository _repository;

    public GetWidgetQuery(IWidgetRepository repository)
    {
        _repository = repository;
    }

    public async Task<Widget> Execute(
        GetWidget message, 
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(message.Id, cancellationToken);
    }
}
```

#### Synchronous Handlers

For synchronous operations, implement `IHandler<TRequest, TResponse>`:

```csharp
public class ValidateWidget : IHandler<Widget, bool>
{
    public bool Execute(Widget message)
    {
        return !string.IsNullOrEmpty(message.Name);
    }
}
```

## Domain Events

Publish domain events to notify other parts of your application:

```csharp
public class CreateWidgetHandler : IAsyncHandler<CreateWidgetMessage, CommandResponse<string>>
{
    private readonly IDomainEventPublisher _publisher;

    public CreateWidgetHandler(IDomainEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task<CommandResponse<string>> Execute(
        CreateWidgetMessage message,
        CancellationToken cancellationToken = default)
    {
        var widgetId = Guid.NewGuid().ToString();

        // Publish domain event
        await _publisher.Publish(
            new WidgetCreated { Id = widgetId, Name = message.Name },
            cancellationToken);

        return Response.Ok(widgetId);
    }
}
```

### Domain Event Handlers

Implement `IDomainEvent<TMessage>` to handle domain events:

```csharp
public class WidgetCreatedHandler : IDomainEvent<WidgetCreated>
{
    private readonly IEmailService _emailService;
    public event EventHandler<DomainEventArgs>? OnComplete;

    public WidgetCreatedHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Execute(WidgetCreated message)
    {
        await _emailService.SendNotificationAsync($"Widget {message.Name} created");
        
        OnComplete?.Invoke(this, new DomainEventArgs 
        { 
            Success = true, 
            Message = "Notification sent" 
        });
    }
}
```

## Pipeline Middleware (NEW in 1.0.10)

Add cross-cutting concerns to domain events using middleware pipelines powered by [abes.GenericPipeline](https://github.com/tomlazelle/pipeline).

### Configure Pipeline

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddCommandQuery(typeof(Startup).Assembly);
    
    // Register middleware
    services
        .AddDomainEventMiddleware<LoggingMiddleware<WidgetCreated>>()
        .AddDomainEventMiddleware<ValidationMiddleware<WidgetCreated>>();
    
    // Configure pipeline for specific message type
    services.AddDomainEventPipeline<WidgetCreated>(builder =>
    {
        builder.Use<ValidationMiddleware<WidgetCreated>>();
        builder.Use<LoggingMiddleware<WidgetCreated>>();
    });
}
```

### Create Middleware

```csharp
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
        _logger.LogInformation("Processing: {MessageType}", typeof(TMessage).Name);
        
        await next(context); // Execute next middleware or handler
        
        _logger.LogInformation("Completed: {MessageType}, Success: {Success}", 
            typeof(TMessage).Name, 
            context.Success);
    }
}
```

**See [PIPELINE_GUIDE.md](PIPELINE_GUIDE.md) for comprehensive pipeline documentation.**

## Response Helpers

Use the static `Response` class to create command responses:

```csharp
// Success with data
return Response.Ok(widgetId);

// Success without data
return Response.Ok();

// Failure with single error
return Response.Failed<string>("Widget not found");

// Failure with multiple errors
return Response.Failed<string>(new List<string> { "Error 1", "Error 2" });

// Failure with exception
return Response.Failed<string>(exception);
return Response.Failed<string>("Custom message", exception);
```

## Core Interfaces

### IBroker
```csharp
public interface IBroker
{
    Task<TResponse> HandleAsync<TRequest, TResponse>(
        TRequest message, 
        CancellationToken cancellationToken) where TRequest : IMessage;
    
    TResponse Handle<TRequest, TResponse>(
        TRequest message) where TRequest : IMessage;
}
```

### IAsyncHandler
```csharp
public interface IAsyncHandler<in TRequest, TResponse> where TRequest : IMessage
{
    Task<TResponse> Execute(TRequest message, CancellationToken cancellationToken);
}
```

### IHandler
```csharp
public interface IHandler<in TRequest, out TResponse> where TRequest : IMessage
{
    TResponse Execute(TRequest message);
}
```

### IDomainEventPublisher
```csharp
public interface IDomainEventPublisher
{
    event EventHandler MessageSent;
    event EventHandler<DomainEventArgs> MessageResult;
    
    Task Publish<TMessageType>(
        TMessageType message, 
        CancellationToken cancellationToken);
}
```

## Best Practices

✅ **Commands** - Modify state, return `CommandResponse<T>`  
✅ **Queries** - Read-only, return domain models  
✅ **Validation** - Validate in handlers or use pipeline middleware  
✅ **Error Handling** - Return `Response.Failed()` instead of throwing  
✅ **CancellationToken** - Always accept and pass cancellation tokens  
✅ **Domain Events** - Use for cross-cutting concerns and notifications  
✅ **Pipelines** - Add logging, validation, authorization as middleware  

## Sample Application

See the [sample folder](sample/) for a complete working example demonstrating:
- Command and query handlers
- Domain event publishing
- Pipeline middleware
- ASP.NET Core integration

## Breaking Changes

### Version 1.0.4+
- Renamed `ICommandBroker` → `IBroker`
- Unified command/query interfaces to `IHandler`/`IAsyncHandler`
- Removed separate command/query base classes
- Messages must implement `IMessage` marker interface

### Version 1.0.10
- Added pipeline middleware support via `abes.GenericPipeline`
- Updated `Microsoft.Extensions.DependencyInjection.Abstractions` to 10.0.1
- Enhanced error handling with detailed exception messages

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Links

- [NuGet Package](https://www.nuget.org/packages/CommandQuery.Framing)
- [GitHub Repository](https://github.com/tomlazelle/CommandQuery.Framing)
- [Pipeline Guide](PIPELINE_GUIDE.md)
- [Sample Application](sample/)
