using System;
using System.Threading.Tasks;
using CommandQuery.Framing;
using CommandQueryApiSample.Domain.Messages;

namespace CommandQueryApiSample.Domain.MessageHandlers
{
    public class WidgetCreatedHandler:IDomainEvent<WidgetCreated>
    {
        public event EventHandler<DomainEventArgs> OnComplete;
        public async Task Execute(WidgetCreated message)
        {
            OnComplete?.Invoke(this,new DomainEventArgs
                                    {
                                        Message = "worked",
                                        Success = true
                                    });
            await Task.CompletedTask;
        }
    }
}