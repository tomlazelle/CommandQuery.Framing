using CommandQuery.Framing;

namespace CommandQueryApiSample.Domain.Messages
{
    public class GetWidget:IRqstMessage
    {
        public string Id { get; set; }
    }
}