﻿using System;
using System.Threading.Tasks;
using ConsoleApp.Events;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Sillycore.RabbitMq.Attributes;

namespace ConsoleApp.Consumers
{
    [Consumer(QueueName = "sillycore.do_something_when_some_event_occured")]
    public class TestConsumer : IConsumer<SomeEvent>
    {
        private readonly IConfiguration _configuration;

        public TestConsumer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task Consume(ConsumeContext<SomeEvent> context)
        {
            await Console.Out.WriteLineAsync($"{context.Message.Data} sillycore.do_something_when_some_event_occured");
        }
    }
}