using CommandQuery.Framing;
using CommandQueryApiSample.Domain.Messages;

namespace CommandQueryApiSample.Domain.MessageHandlers
{
    public class WidgetCreatedHandler:IDomainEvent<WidgetCreated>
    {
        public void Execute(WidgetCreated message)
        {
            //do some work here
        }
    }
}