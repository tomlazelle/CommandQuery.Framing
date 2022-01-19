using CommandQuery.Framing;

namespace CommandQueryApiSample.Domain.Messages
{
    public class GetWidget:IMessage
    {
        public string Id { get; set; }
    }
}