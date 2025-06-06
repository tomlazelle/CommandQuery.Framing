using System.Reflection;
using CommandQuery.Framing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace CommandTests.ScannerTests;

public class CommandQueryExtensionsTests
{
    private readonly IServiceCollection _services;
    private readonly Assembly _testAssembly;

    public CommandQueryExtensionsTests()
    {
        _services = new ServiceCollection();
        _testAssembly = typeof(TestAsyncHandler).Assembly;
    }

    [Fact]
    public void AddCommandQuery_Registers_Singletons_And_Handlers()
    {
        // Act
        _services.AddCommandQuery(_testAssembly);
        var provider = _services.BuildServiceProvider();

        // Assert singletons
        var broker = provider.GetService<IBroker>();
        var publisher = provider.GetService<IDomainEventPublisher>();

        broker.ShouldNotBeNull();
        publisher.ShouldNotBeNull();

        // Assert handler implementations
        provider.GetService<IAsyncHandler<SampleCommand, CommandResponse<SampleResult>>>().ShouldBeOfType<TestAsyncHandler>();
        provider.GetService<IHandler<SampleCommand, SampleResult>>().ShouldBeOfType<TestHandler>();
    }

    [Fact]
    public void AddCommandQuery_Returns_Same_ServiceCollection()
    {
        // Act
        var result = _services.AddCommandQuery(_testAssembly);

        // Assert
        result.ShouldBeSameAs(_services);
    }
}

public class SampleCommand : IMessage
{
}

public class SampleResult
{
}

public class TestAsyncHandler : IAsyncHandler<SampleCommand, CommandResponse<SampleResult>>
{
    public async Task<CommandResponse<SampleResult>> Execute(SampleCommand message, CancellationToken cancellationToken)
    {
        return await Task.FromResult(Response.Ok(new SampleResult()));
    }
}

public class TestHandler : IHandler<SampleCommand, SampleResult>
{
    public SampleResult Execute(SampleCommand message)
    {
        throw new NotImplementedException();
    }
}