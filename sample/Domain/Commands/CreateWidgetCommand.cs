using System;
using System.Threading.Tasks;
using CommandQuery.Framing;
using CommandQueryApiSample.Domain.Requests;

namespace CommandQueryApiSample.Domain.Commands
{
    public class CreateWidgetCommand : AsyncCommandHandler<CreateWidget, CommandResponse>
    {
        public CreateWidgetCommand(IDomainEventPublisher domainEventPublisher) : base(domainEventPublisher)
        {
        }

        public override async Task<CommandResponse> Execute(CreateWidget message)
        {
            return CommandResponse.Okay(Guid.NewGuid().ToString());
        }
    }
}