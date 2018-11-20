namespace CommandQuery.Framing
{
    public interface IDomainEvent<T>
    {
        void Execute(T message);
    }
}