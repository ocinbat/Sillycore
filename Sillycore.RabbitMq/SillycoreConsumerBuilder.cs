using System;
using System.Collections.Generic;
using System.Text;
using GreenPipes;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.Logging;

namespace Sillycore.RabbitMq
{
    public class SillycoreConsumerBuilder<T> where T: class, IConsumer
    {
        private readonly string _queue;
        private readonly SillycoreRabbitMqBuilder _sillycoreRabbitMqBuilder;

        private ushort _prefetchCount = 32;
        private int? _concurrencyLimit;
        private int? _numberOfImmediateRetries;

        public SillycoreConsumerBuilder(SillycoreRabbitMqBuilder sillycoreRabbitMqBuilder, string queue)
        {
            _sillycoreRabbitMqBuilder = sillycoreRabbitMqBuilder;
            _queue = queue;
        }

        public SillycoreConsumerBuilder<T> WithPrefetchCount(ushort prefetchCount)
        {
            _prefetchCount = prefetchCount;
            return this;
        }

        public SillycoreConsumerBuilder<T> WithConcurrenyLimit(int concurrencyLimit)
        {
            _concurrencyLimit = concurrencyLimit;
            return this;
        }

        public SillycoreConsumerBuilder<T> WithImmediateRetry(int numberOfRetries)
        {
            _numberOfImmediateRetries = numberOfRetries;
            return this;
        }

        public SillycoreRabbitMqBuilder Then()
        {
            ConsumerConfiguration configuration = new ConsumerConfiguration
            {
                Queue = _queue,
                Type = typeof(T),
                ConfigureAction = (c) =>
                {
                    ICachedConfigurator configurator = new SillycoreConsumerConfigurator<T>();
                    configurator.Configure(c, null);
                    c.PrefetchCount = _prefetchCount;
                    if (_concurrencyLimit.HasValue)
                    {
                        c.UseConcurrencyLimit(_concurrencyLimit.Value);
                    }

                    if (_numberOfImmediateRetries.HasValue)
                    {
                        c.UseRetry(retryConfigurator => { retryConfigurator.Immediate(_numberOfImmediateRetries.Value); });
                    }
                }
            };

            _sillycoreRabbitMqBuilder.AddConsumerConfiguration(configuration);

            return _sillycoreRabbitMqBuilder;
        }
    }
}
