using System;
using System.Threading.Tasks;
using CommandQuery.Framing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace CommandTests.DomainTests
{
    public class CanGetMessageFromDomainPublisherTest
    {
        public async Task can_get_message_from_domain_publisher()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IDomainEventPublisher, DomainEventPublisher>();
            serviceCollection.AddTransient<IDomainEvent<TestDomainEventMessage>, TestDomainEvent>();
            serviceCollection.AddTransient<IDomainEvent<TestDomainEventMessage>, TestDomainEventTwo>();
            var provider = serviceCollection.BuildServiceProvider();

            var publisher = provider.GetService<IDomainEventPublisher>();

            var sentCnt = 0;
            var resultCnt = 0;
            
            publisher.MessageSent += (sender, args) => sentCnt++;
            
            publisher.MessageResult += (sender, args) => resultCnt++;

            await publisher.Publish(new TestDomainEventMessage());
            
            sentCnt.ShouldBe(2);
            resultCnt.ShouldBe(2);
        }

    }

    public class TestDomainEventMessage
    {

    }

    public class TestDomainEvent : IDomainEvent<TestDomainEventMessage>
    {
        public event EventHandler<DomainEventArgs> OnComplete;
        
        public async Task Execute(TestDomainEventMessage message)
        {
            OnComplete(this,
                       new DomainEventArgs
                       {
                           Message = "Completed",
                           Success = true
                       });
        
            await Task.CompletedTask;
        }
    }
    
    public class TestDomainEventTwo : IDomainEvent<TestDomainEventMessage>
    {
        public event EventHandler<DomainEventArgs> OnComplete;
        
        public async Task Execute(TestDomainEventMessage message)
        {
            OnComplete(this,
                       new DomainEventArgs
                       {
                           Message = "Completed",
                           Success = true
                       });
        
            await Task.CompletedTask;
        }
    }
}