using System;
using System.Threading.Tasks;
using ConsoleApp.Events;
using MassTransit;
using Sillycore.RabbitMq.Attributes;

namespace ConsoleApp.Consumers
{
    [Consumer(QueueName = "sillycore.do_something_when_some_event_occured", ConcurrenyLimit = 1)]
    public class TestConsumer : IConsumer<SomeEvent>
    {
        public string Test { get; set; }

        public async Task Consume(ConsumeContext<SomeEvent> context)
        {
            await Console.Out.WriteLineAsync($"Event:{context.Message.Data} processed.");
            Test = context.Message.Data;
        }
    }
}