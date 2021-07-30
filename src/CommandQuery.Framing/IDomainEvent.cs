using System;
using System.Threading.Tasks;

namespace CommandQuery.Framing
{
    public interface IDomainEvent<T>
    {
        event EventHandler<DomainEventArgs> OnComplete;
        Task Execute(T message);
    }

    public class DomainEventArgs:EventArgs
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}