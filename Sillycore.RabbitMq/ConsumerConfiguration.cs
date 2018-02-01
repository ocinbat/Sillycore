using System;
using MassTransit.RabbitMqTransport;

namespace Sillycore.RabbitMq
{
    internal class ConsumerConfiguration
    {
        public Action<IRabbitMqReceiveEndpointConfigurator> ConfigureAction { get; set; }

        public string Queue { get; set; }

        public Type Type { get; set; }
    }
}