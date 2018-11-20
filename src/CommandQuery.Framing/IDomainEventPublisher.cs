namespace CommandQuery.Framing
{
    public interface IDomainEventPublisher
    {
        void Publish<TMessageType>(TMessageType message);
    }
}