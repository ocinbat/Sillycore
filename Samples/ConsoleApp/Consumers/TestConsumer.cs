using System;
using System.Threading.Tasks;
using App.Metrics;
using ConsoleApp.Events;
using ConsoleApp.HealthCheckers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Sillycore.RabbitMq.Attributes;

namespace ConsoleApp.Consumers
{
    [Consumer(QueueName = "sillycore.do_something_when_some_event_occured")]
    public class TestConsumer : IConsumer<SomeEvent>
    {
        private readonly IMetrics _metrics;
        private readonly IConfiguration _configuration;

        public TestConsumer(IConfiguration configuration, IMetrics metrics)
        {
            _configuration = configuration;
            _metrics = metrics;
        }

        public async Task Consume(ConsumeContext<SomeEvent> context)
        {
            _metrics.Measure.Counter.Increment(MetricsRegistry.GlobalCounter, new MetricTags("MethodName", "Consume"));
            using (_metrics.Measure.Timer.Time(MetricsRegistry.GlobalTimer,
                new MetricTags("MethodName", "Consume")))
            {
                await Console.Out.WriteLineAsync(
                    $"{context.Message.Data} sillycore.do_something_when_some_event_occured");
            }
        }
    }
}