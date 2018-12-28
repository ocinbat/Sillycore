using System;

namespace Sillycore.RabbitMq.Attributes
{
    public class ConsumerAttribute : Attribute
    {
        public string QueueName { get; set; }
        public string ExchangeName { get; set; }
        public ushort PrefetchCount { get; set; }
        public int ConcurrenyLimit { get; set; }
        public int ImmediateRetry { get; set; }
        public string RabbitMq { get; set; } = "RabbitMq";
    }
}