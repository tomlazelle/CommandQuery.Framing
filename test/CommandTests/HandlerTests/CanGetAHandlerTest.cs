using System.Threading;
using System.Threading.Tasks;
using CommandQuery.Framing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace CommandTests.HandlerTests;

public class CanGetAHandlerTest
{
    [Fact]
    public async Task can_get_a_handler()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddCommandQuery(typeof(CanGetAHandlerTest).Assembly);
        var provider = serviceCollection.BuildServiceProvider();

        var broker = provider.GetService<IBroker>();

        var cancellationToken = new CancellationToken();

        var result =
            await broker.HandleAsync<TestHandlerMessage, CommandResponse<string>>(new TestHandlerMessage(),
                cancellationToken);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<CommandResponse<string>>();
        result.Data.ShouldBe("I did it");
    }
}

public class TestHandlerMessage : IMessage
{
}

public class TestHandler : IAsyncHandler<TestHandlerMessage, CommandResponse<string>>
{
    public async Task<CommandResponse<string>> Execute(TestHandlerMessage message,
        CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(Response.Ok("I did it"));
    }
}