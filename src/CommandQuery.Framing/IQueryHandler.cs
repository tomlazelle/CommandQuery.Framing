namespace CommandQuery.Framing
{
    public interface IQueryHandler<TMessageType, TReturnType>
    {
        TReturnType Execute(TMessageType message);
    }
}