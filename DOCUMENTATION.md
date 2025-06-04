# CommandQuery.Framing Documentation

## Overview
CommandQuery.Framing is a framework designed to simplify the implementation of the Command Query Responsibility Segregation (CQRS) pattern. It provides abstractions and utilities for handling commands, queries, and domain events in a structured and scalable manner.

## Features
- **Command and Query Handling**: Define and handle commands and queries with ease.
- **Domain Event Publishing**: Publish and handle domain events.
- **Extensibility**: Easily extend the framework to suit your application's needs.
- **Assembly Scanning**: Automatically discover handlers and other components using assembly scanning.

## Getting Started

### Installation
To use CommandQuery.Framing in your project, you can install the NuGet package:

```bash
nuget install CommandQuery.Framing
```

Alternatively, you can reference the `.nupkg` file located in the `nuget/` directory of this repository.

### Setup
1. Add the `CommandQuery.Framing` package to your project.
2. Configure the framework in your application's startup file (e.g., `Startup.cs`).

### Example Usage

#### Defining a Command
```csharp
public record CreateWidgetMessage(string Name) : IMessage;
```

#### Creating a Command Handler
```csharp
public class CreateWidgetHandler : IAsyncHandler<CreateWidgetMessage, CommandResponse<string>>
{
    private readonly IDomainEventPublisher _publisher;

    public CreateWidgetHandler(IDomainEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task<CommandResponse<string>> Execute(CreateWidgetMessage message, CancellationToken cancellationToken = default)
    {
        var response = Guid.NewGuid().ToString();

        _publisher.MessageResult += (sender, eventArgs) =>
        {
            response += $"Name: {message.Name} processed with Success={eventArgs.Success}";
        };

        await _publisher.Publish(new WidgetCreated { Name = message.Name }, cancellationToken);

        return Response.Ok(response);
    }
}
```

#### Defining a Query
```csharp
public class GetWidgetQuery : IAsyncHandler<GetWidget, Widget>
{
    public async Task<Widget> Execute(GetWidget message, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(new Widget { Id = message.Id });
    }
}
```

#### Registering Handlers
Use the `AddCommandQuery` extension method to automatically register handlers:

```csharp
services.AddCommandQuery(typeof(Startup).Assembly);
```

#### Executing a Command
```csharp
var command = new CreateWidgetMessage("Sample Widget");
var response = await broker.HandleAsync<CreateWidgetMessage, CommandResponse<string>>(command);

if (response.Success)
{
    Console.WriteLine($"Command executed successfully: {response.Data}");
}
else
{
    Console.WriteLine($"Command failed: {response.Message}");
}
```

#### Executing a Query
```csharp
var query = new GetWidget { Id = "123" };
var widget = await broker.HandleAsync<GetWidget, Widget>(query);
Console.WriteLine($"Widget ID: {widget.Id}");
```

## Project Structure

### `src/CommandQuery.Framing`
Contains the core framework code, including:
- `Broker.cs`: Handles the dispatching of commands and queries.
- `CommandResponse.cs`: Represents the response of a command.
- `DomainEventPublisher.cs`: Publishes domain events.
- `IAsyncHandler.cs`: Interface for asynchronous handlers.
- `IMessage.cs`: Interface for messages (commands and queries).

### `sample/`
Contains a sample application demonstrating the usage of the framework.

### `test/`
Contains unit tests for the framework.

## Contributing
Contributions are welcome! Please follow these steps:
1. Fork the repository.
2. Create a new branch for your feature or bugfix.
3. Submit a pull request with a detailed description of your changes.

## License
This project is licensed under the terms of the [MIT License](LICENSE).

## Support
For questions or support, please open an issue in the repository or contact the maintainers.