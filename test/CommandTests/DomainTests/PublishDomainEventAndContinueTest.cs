using System;
using System.Threading;
using System.Threading.Tasks;
using CommandQuery.Framing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace CommandTests.DomainTests;

public class PublishDomainEventAndContinueTest
{
    [Fact]
    public async Task can_continue_after_publishing_domain_event()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IDomainEventPublisher, DomainEventPublisher>();
        serviceCollection.AddTransient<IDomainEvent<TestDomainEventMessage>, TestDomainEvent>();
        serviceCollection.AddTransient<IDomainEvent<TestDomainEventMessage>, TestDomainEventTwo>();
        var provider = serviceCollection.BuildServiceProvider();

        var publisher = provider.GetService<IDomainEventPublisher>();

        var sentCnt = 0;
        var resultCnt = 0;

        publisher.MessageSent += (sender, args) =>
        {
            sentCnt++;
            Console.WriteLine($"Sent {sentCnt}");
        };

        publisher.MessageResult += (sender, args) =>
        {
            resultCnt++;
            Console.WriteLine($"Result {resultCnt}");
        };

        Console.WriteLine("Publishing");
        await publisher.Publish(new TestDomainEventMessage(), new CancellationToken());

        // simulate a delay
        Console.WriteLine("Delaying");
        await Task.Delay(1000);
        Console.WriteLine("Delay complete");
        sentCnt++;
        resultCnt++;


        sentCnt.ShouldBe(3);
        resultCnt.ShouldBe(3);
    }
}