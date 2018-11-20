namespace CommandQuery.Framing
{
    public interface ICommandHandler<TRequest,TResponse>
    {
        TResponse Execute(TRequest message);
    }
}