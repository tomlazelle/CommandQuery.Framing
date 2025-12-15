using CommandQuery.Framing;
using CommandTests.Configuration;
using GenericPipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CommandTests.HandlerTests;

public class AsyncHandlerPipelineTests
{
    [Fact]
    public async Task AsyncHandlerPipeline_ExecutesMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<IServiceScopeFactory>(sp => sp.GetRequiredService<IServiceScopeFactory>());
        
        services.AddHandlerMiddleware<TestAsyncLoggingMiddleware>();
        services.AddAsyncHandlerPipeline<TestRequest, TestResponse>(builder =>
        {
            builder.Use<TestAsyncLoggingMiddleware>();
        });
        
        services.AddTransient<IAsyncHandler<TestRequest, TestResponse>, TestAsyncHandler>();
        services.AddTransient<IBroker, Broker>();

        var provider = services.BuildServiceProvider();
        var broker = provider.GetRequiredService<IBroker>();

        // Act
        var response = await broker.HandleAsync<TestRequest, TestResponse>(new TestRequest { Value = "test" }, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("test_processed", response.Result);
        Assert.True(TestAsyncLoggingMiddleware.BeforeCalled);
        Assert.True(TestAsyncLoggingMiddleware.AfterCalled);
    }

    [Fact]
    public async Task AsyncHandlerPipeline_CanShortCircuit()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<IServiceScopeFactory>(sp => sp.GetRequiredService<IServiceScopeFactory>());
        
        services.AddHandlerMiddleware<TestAsyncShortCircuitMiddleware>();
        services.AddAsyncHandlerPipeline<TestRequest, TestResponse>(builder =>
        {
            builder.Use<TestAsyncShortCircuitMiddleware>();
        });
        
        services.AddTransient<IAsyncHandler<TestRequest, TestResponse>, TestAsyncHandler>();
        services.AddTransient<IBroker, Broker>();

        var provider = services.BuildServiceProvider();
        var broker = provider.GetRequiredService<IBroker>();

        // Act
        var response = await broker.HandleAsync<TestRequest, TestResponse>(new TestRequest { Value = "short-circuit" }, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("short-circuited", response.Result);
    }

    [Fact]
    public async Task AsyncHandlerPipeline_WithoutPipeline_ExecutesNormally()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<IServiceScopeFactory>(sp => sp.GetRequiredService<IServiceScopeFactory>());
        
        // No pipeline configured
        services.AddTransient<IAsyncHandler<TestRequest, TestResponse>, TestAsyncHandler>();
        services.AddTransient<IBroker, Broker>();

        var provider = services.BuildServiceProvider();
        var broker = provider.GetRequiredService<IBroker>();

        // Act
        var response = await broker.HandleAsync<TestRequest, TestResponse>(new TestRequest { Value = "test" }, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("test_processed", response.Result);
    }
}

// Test classes
public class TestRequest : IMessage
{
    public string Value { get; set; }
}

public class TestResponse
{
    public string Result { get; set; }
}

public class TestAsyncHandler : IAsyncHandler<TestRequest, TestResponse>
{
    public async Task<TestResponse> Execute(TestRequest message, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);
        return new TestResponse { Result = message.Value + "_processed" };
    }
}

public class TestAsyncLoggingMiddleware : IPipelineMiddleware<AsyncHandlerContext<TestRequest, TestResponse>>
{
    public static bool BeforeCalled { get; set; }
    public static bool AfterCalled { get; set; }

    public async ValueTask InvokeAsync(
        AsyncHandlerContext<TestRequest, TestResponse> context,
        PipelineDelegate<AsyncHandlerContext<TestRequest, TestResponse>> next)
    {
        BeforeCalled = true;
        await next(context);
        AfterCalled = true;
    }
}

public class TestAsyncShortCircuitMiddleware : IPipelineMiddleware<AsyncHandlerContext<TestRequest, TestResponse>>
{
    public ValueTask InvokeAsync(
        AsyncHandlerContext<TestRequest, TestResponse> context,
        PipelineDelegate<AsyncHandlerContext<TestRequest, TestResponse>> next)
    {
        if (context.Request.Value == "short-circuit")
        {
            context.ShouldContinue = false;
            context.Response = new TestResponse { Result = "short-circuited" };
            return ValueTask.CompletedTask;
        }

        return next(context);
    }
}
