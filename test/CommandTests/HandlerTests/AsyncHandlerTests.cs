using AutoFixture;
using CommandQuery.Framing;
using NSubstitute;

namespace CommandTests.HandlerTests;

public class AsyncHandlerTests
{
    private readonly IFixture _fixture;

    public AsyncHandlerTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task Execute_ShouldReturnExpectedResponse()
    {
        // Arrange
        var request = _fixture.Create<TestRequest>();
        var expectedResponse = _fixture.Create<TestResponse>();

        var handler = Substitute.For<IAsyncHandler<TestRequest, TestResponse>>();
        handler.Execute(request, Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var response = await handler.Execute(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(expectedResponse.Result, response.Result);
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenRequestIsNull()
    {
        // Arrange
        var handler = Substitute.For<IAsyncHandler<TestRequest, TestResponse>>();
        handler.Execute(null, Arg.Any<CancellationToken>())
            .Returns<Task<TestResponse>>(_ => throw new ArgumentNullException(nameof(TestRequest)));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Execute(null, CancellationToken.None));
    }

    [Fact]
    public async Task Execute_ShouldRespectCancellationToken()
    {
        // Arrange
        var request = _fixture.Create<TestRequest>();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var handler = Substitute.For<IAsyncHandler<TestRequest, TestResponse>>();
        handler.Execute(Arg.Any<TestRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task<TestResponse>>(_ => throw new TaskCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => handler.Execute(request, cancellationTokenSource.Token));
    }

    public class TestRequest : IMessage
    {
        public string Data { get; set; }
    }

    public class TestResponse
    {
        public string Result { get; set; }
    }
}