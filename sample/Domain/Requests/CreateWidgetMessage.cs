using CommandQuery.Framing;

namespace CommandQueryApiSample.Domain.Requests;

public record CreateWidgetMessage(string Name) : IMessage;

public record CreateWidgetV2(string Name) : IMessage;

public class CreateRecord
{
    public string Name { get; set; } = string.Empty;
}