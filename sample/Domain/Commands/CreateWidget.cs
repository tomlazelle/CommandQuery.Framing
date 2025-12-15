using CommandQuery.Framing;
using CommandQueryApiSample.Domain.Messages;
using CommandQueryApiSample.Domain.Requests;

namespace CommandQueryApiSample.Domain.Commands;

//v1
public class CreateWidget : IAsyncHandler<CreateWidgetMessage, CommandResponse<string>>
{
    private readonly IDomainEventPublisher _publisher;

    public CreateWidget(IDomainEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task<CommandResponse<string>> Execute(CreateWidgetMessage message,
        CancellationToken cancellationToken = default)
    {
        var response = Guid.NewGuid().ToString();

        _publisher.MessageResult += (sender, eventargs) =>
        {
            response += $"Name: {message.Name} message was sent and processed with Success={eventargs.Success}";
        };

        await _publisher.Publish(new WidgetCreated { Name = message.Name }, cancellationToken);

        return Response.Ok(response);
    }
}