using System;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.Scoping;

namespace Sillycore.RabbitMq
{
    public class SillycoreConsumerConfigurator<T> : ICachedConfigurator where T : class, IConsumer
    {
        private static readonly SillycoreConsumerScopeProvider ConsumerScopeProvider = new SillycoreConsumerScopeProvider();

        public void Configure(IReceiveEndpointConfigurator configurator, IServiceProvider serviceProvider)
        {
            configurator.Consumer<T>(new ScopeConsumerFactory<T>(ConsumerScopeProvider), (Action<IConsumerConfigurator<T>>)null);
        }
    }
}