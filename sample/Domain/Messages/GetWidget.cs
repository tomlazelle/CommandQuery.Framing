using CommandQuery.Framing;

namespace CommandQueryApiSample.Domain.Messages
{
    public class GetWidget:IQueryMessage
    {
        public string Id { get; set; }
    }
}