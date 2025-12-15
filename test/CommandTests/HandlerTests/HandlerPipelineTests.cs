using CommandQuery.Framing;
using CommandTests.Configuration;
using GenericPipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CommandTests.HandlerTests;

public class HandlerPipelineTests
{
    [Fact]
    public void HandlerPipeline_ExecutesMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<IServiceScopeFactory>(sp => sp.GetRequiredService<IServiceScopeFactory>());
        
        services.AddHandlerMiddleware<TestSyncLoggingMiddleware>();
        services.AddHandlerPipeline<TestSyncRequest, TestSyncResponse>(builder =>
        {
            builder.Use<TestSyncLoggingMiddleware>();
        });
        
        services.AddTransient<IHandler<TestSyncRequest, TestSyncResponse>, TestSyncHandler>();
        services.AddTransient<IBroker, Broker>();

        var provider = services.BuildServiceProvider();
        var broker = provider.GetRequiredService<IBroker>();

        // Act
        var response = broker.Handle<TestSyncRequest, TestSyncResponse>(new TestSyncRequest { Value = "test" });

        // Assert
        Assert.NotNull(response);
        Assert.Equal("test_processed", response.Result);
        Assert.True(TestSyncLoggingMiddleware.BeforeCalled);
        Assert.True(TestSyncLoggingMiddleware.AfterCalled);
    }

    [Fact]
    public void HandlerPipeline_CanShortCircuit()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<IServiceScopeFactory>(sp => sp.GetRequiredService<IServiceScopeFactory>());
        
        services.AddHandlerMiddleware<TestSyncShortCircuitMiddleware>();
        services.AddHandlerPipeline<TestSyncRequest, TestSyncResponse>(builder =>
        {
            builder.Use<TestSyncShortCircuitMiddleware>();
        });
        
        services.AddTransient<IHandler<TestSyncRequest, TestSyncResponse>, TestSyncHandler>();
        services.AddTransient<IBroker, Broker>();

        var provider = services.BuildServiceProvider();
        var broker = provider.GetRequiredService<IBroker>();

        // Act
        var response = broker.Handle<TestSyncRequest, TestSyncResponse>(new TestSyncRequest { Value = "short-circuit" });

        // Assert
        Assert.NotNull(response);
        Assert.Equal("short-circuited", response.Result);
    }

    [Fact]
    public void HandlerPipeline_WithoutPipeline_ExecutesNormally()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<IServiceScopeFactory>(sp => sp.GetRequiredService<IServiceScopeFactory>());
        
        // No pipeline configured
        services.AddTransient<IHandler<TestSyncRequest, TestSyncResponse>, TestSyncHandler>();
        services.AddTransient<IBroker, Broker>();

        var provider = services.BuildServiceProvider();
        var broker = provider.GetRequiredService<IBroker>();

        // Act
        var response = broker.Handle<TestSyncRequest, TestSyncResponse>(new TestSyncRequest { Value = "test" });

        // Assert
        Assert.NotNull(response);
        Assert.Equal("test_processed", response.Result);
    }
}

// Test classes
public class TestSyncRequest : IMessage
{
    public string Value { get; set; }
}

public class TestSyncResponse
{
    public string Result { get; set; }
}

public class TestSyncHandler : IHandler<TestSyncRequest, TestSyncResponse>
{
    public TestSyncResponse Execute(TestSyncRequest message)
    {
        return new TestSyncResponse { Result = message.Value + "_processed" };
    }
}

public class TestSyncLoggingMiddleware : ISyncPipelineMiddleware<HandlerContext<TestSyncRequest, TestSyncResponse>>
{
    public static bool BeforeCalled { get; set; }
    public static bool AfterCalled { get; set; }

    public void Invoke(
        HandlerContext<TestSyncRequest, TestSyncResponse> context,
        Action next)
    {
        BeforeCalled = true;
        next();
        AfterCalled = true;
    }
}

public class TestSyncShortCircuitMiddleware : ISyncPipelineMiddleware<HandlerContext<TestSyncRequest, TestSyncResponse>>
{
    public void Invoke(
        HandlerContext<TestSyncRequest, TestSyncResponse> context,
        Action next)
    {
        if (context.Request.Value == "short-circuit")
        {
            context.ShouldContinue = false;
            context.Response = new TestSyncResponse { Result = "short-circuited" };
            return;
        }

        next();
    }
}
