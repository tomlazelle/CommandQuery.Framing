using System;
using System.Threading.Tasks;
using CommandQuery.Framing;
using CommandQueryApiSample.Domain.Requests;

namespace CommandQueryApiSample.Domain.Commands
{
    public class CreateWidget : AsyncHandler<Requests.CreateWidget, CommandResponse>
    {
        public CreateWidget(IDomainEventPublisher domainEventPublisher) : base(domainEventPublisher)
        {
        }

        public override async Task<CommandResponse> Execute(Requests.CreateWidget message)
        {
            return CommandResponse.Okay(Guid.NewGuid().ToString());
        }
    }
}