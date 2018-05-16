using System;
using System.Threading.Tasks;
using ConsoleApp.Commands;
using ConsoleApp.Events;
using MassTransit;
using Sillycore.RabbitMq.Attributes;

namespace ConsoleApp.Consumers
{
    [Consumer(QueueName = "superb_command_queue")]
    public class SuperbConsumer : IConsumer<SomeCommand>
    {
        public async Task Consume(ConsumeContext<SomeCommand> context)
        {
            await Console.Out.WriteLineAsync($"{context.Message.Data} superb_command_queue.");
        }
    }
}