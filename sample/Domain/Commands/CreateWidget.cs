using System;
using System.Threading.Tasks;
using CommandQuery.Framing;
using CommandQueryApiSample.Domain.Messages;
using CommandQueryApiSample.Domain.Requests;

namespace CommandQueryApiSample.Domain.Commands
{
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
}