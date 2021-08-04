# Command Query Framework

Breaking changes for version 1.0.4 and higher
### Objects
#### Commands
* IBroker
* Broker
* IRqstMessage
* IHandler
* IAsyncHandler

#### Domain Event
* IDomainEvent
* IDomainEventPublisher

#### Static Response
* Response

### Setup
```cs
public void ConfigureServices(IServiceCollection services)
{
    /// this will add required interface
    /// this will also search your assemblies for
    /// types that can be handled
    services
        .AddCommandQuery(<Your assemblies here>);
}
```

### Calling a Command or Query
Inject the IBroker into your class
Call
```cs
public class TestController:Controller
{
	private ICommandBroker _broker;

	public TestController(IBroker broker)
	{
		_broker = broker;
	}

    [Route("/widget")]
    [HttpPost]
    public asycn Task<IActionResult> CreateWidget(Widget request)
    {
        // execute command
        var result = await _broker.HandleAsync<Widget, CommandResponse<string>>(request);

        //check command result
        if(result.Success)
        {
            // return success
            return Ok(result.Data);
        }

        // throw on failure
        throw new BadRequestException(result.Message, result.Exception);
    }
}
```

### Creating a Command Handler
* A Handler is used for gettting, inserting, updating, or deleting data from your database.
* The DomainEventPublisher is used to publish messages accross your domain.
* Ideally commands should be encapsulated and should not call other commands.
* If you need to query for data then it should be part of the command encapsulating the functionality.
```cs
    public class CreateWidget : IAsyncHandler<CreateWidgetMessage, CommandResponse<string>>
    {
        private readonly IDomainEventPublisher _publisher;

        public CreateWidget(IDomainEventPublisher publisher)
        {
            _publisher = publisher;
        }
        public async Task<CommandResponse<string>> Execute(CreateWidgetMessage message)
        {
            var response = Guid.NewGuid().ToString();

            _publisher.MessageResult += (sender, eventargs) =>
                                        {
                                            response += $" message was sent and processed with Success={eventargs.Success}";
                                        };

            await _publisher.Publish(new WidgetCreated());

            return Response.Ok(response);
        }
    }
```

## <= 1.0.3
### Objects
ICommandHandler
CommandHandler
IAsyncCommandHandler
AsyncCommandHandler
ICommandBroker
CommandBroker
IQueryHandler
QueryHandler
IAsyncQueryHandler
AsyncQueryHandler
IDomainEventPublisher
DomainEventPublisher
ICommandBroker
CommandBroker
CommandResponse
IDomainEvent

### Basic Setup
* Register the CommandBroker
* Register the DomainEventPublisher
* Define and Register the CommandHandlers
* Define and Register the QueryHandlers
```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<ICommandBroker, CommandBroker>();
    service.AddTransient<IDomainEventPublisher, DomainEventPublisher>();
    service.AddTransient<ICommandHandler<CreateMessage,CommandResponse>, CreateNewWidgetCommand>();
    service.AddTransient<IQueryHandler<GetWidget,Widget>, GetWidgetQuery>();
}
```
### Calling a Command or Query
Inject the ICommandBroker into your class
Call
```cs
public class TestController:Controller
{
	private ICommandBroker _commandBroker;

	public TestController(ICommandBroker commandBroker)
	{
		_commandBroker = commandBroker;
	}

    [Route("/widget")]
    [HttpPost]
    public asycn Task<IActionResult> CreateWidget(Widget request)
    {
        // execute command
        var result = await _commandBroker.ExecuteAsync<Widget, CommandResponse>(request);

        //check command result
        if(result.Result == CommandStatus.Success)
        {
            // return success
            return Ok();
        }

        // throw on failure
        throw new BadRequestException(result.Message, result.Exception);
    }
}
```
### Creating a Command Handler
Command Handler is used for inserting, updating, or deleting data from your database.
The DomainEventPublisher is used to publish an outbound message for your domain or the message could be sent to a message queue.
Ideally commands should be encapsulated and should not call other commands or queries.
If you need to query for data then it should be part of the command encapsulating the functionality.
```cs
public class WidgetCommand : AsyncCommandHandler<CreateNewWidgetMessage,CommandResponse>
{
    public WidgetCommand(IDomainEventPublisher) : base(domainEventPublisher)

    public override async Task<CommandResponse> Execute(CreateNewWidgetMessage createNewWidgetMessage)
    {
        //do your work here

        //publish your message accross the domain
        DomainEventPublisher.Publish(new MessageHere());

        return CommandResponse.Okay();
    }
}
```

### Create a Query Handler
Query Handler is used for fetching data from your data source and returning a projection.

```cs
public class GetWidgetDataQuery:IAsyncQueryHandler<GetWidgetDataRequest, WidgetModel>
{
    public async Task<WidgetModel> Execute(GetWidgetDataRequest message)
    {
        //query for your data here
        //do some work

        return result;
    }
}
```
