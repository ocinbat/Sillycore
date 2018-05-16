using System;
using System.Collections.Generic;
using System.Reflection;
using GreenPipes;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sillycore.RabbitMq.Attributes;
using Sillycore.RabbitMq.Configuration;

namespace Sillycore.RabbitMq
{
    public static class SillycoreAppBuilderExtensions
    {
        public static SillycoreAppBuilder UseRabbitMq(this SillycoreAppBuilder builder, string configKey = "RabbitMq")
        {
            RabbitMqConfiguration rabbitMqConfiguration = builder.Configuration.GetSection(configKey).Get<RabbitMqConfiguration>();

            List<ConsumerConfiguration> consumerConfigurations = new List<ConsumerConfiguration>();

            foreach (TypeInfo typeInfo in Assembly.GetEntryAssembly().DefinedTypes)
            {
                ConsumerAttribute consumerAttribute = typeInfo.GetCustomAttribute<ConsumerAttribute>();

                if (consumerAttribute != null)
                {
                    ConsumerConfiguration consumerConfiguration = CreateConsumerConfigurationForType(typeInfo.AsType(), consumerAttribute);
                    consumerConfigurations.Add(consumerConfiguration);
                }
            }

            builder.BeforeBuild(() =>
            {
                IBusControl busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    var host = cfg.Host(new Uri(rabbitMqConfiguration.Url), h =>
                    {
                        h.Username(rabbitMqConfiguration.Username);
                        h.Password(rabbitMqConfiguration.Password);
                    });

                    foreach (ConsumerConfiguration consumerConfiguration in consumerConfigurations)
                    {
                        builder.Services.AddTransient(consumerConfiguration.Type);
                        cfg.ReceiveEndpoint(host, consumerConfiguration.Queue, consumerConfiguration.ConfigureAction);
                    }

                    cfg.UseExtensionsLogging(builder.LoggerFactory);

                    if (rabbitMqConfiguration.Retry != null)
                    {
                        if (rabbitMqConfiguration.Retry.Incremental != null)
                        {
                            cfg.UseRetry(rp => { rp.Incremental(rabbitMqConfiguration.Retry.Incremental.RetryLimit, rabbitMqConfiguration.Retry.Incremental.InitialInterval, rabbitMqConfiguration.Retry.Incremental.IntervalIncrement); });
                        }
                    }

                    if (rabbitMqConfiguration.UseDelayedExchangeMessageScheduler)
                    {
                        cfg.UseDelayedExchangeMessageScheduler();
                    }
                });

                builder.Services.AddSingleton(busControl);
                busControl.Start();
            });

            return builder;
        }

        private static ConsumerConfiguration CreateConsumerConfigurationForType(Type type, ConsumerAttribute consumerAttribute)
        {
            Type consumerConfiguratorGeneric = typeof(SillycoreConsumerConfigurator<>);
            Type consumerConfiguratorType = consumerConfiguratorGeneric.MakeGenericType(type);
            var consumerConfigurator = Activator.CreateInstance(consumerConfiguratorType, null);

            ConsumerConfiguration configuration = new ConsumerConfiguration
            {
                Queue = consumerAttribute.QueueName,
                Type = type,
                ConfigureAction = (c) =>
                {
                    ICachedConfigurator configurator = (ICachedConfigurator)consumerConfigurator;
                    configurator.Configure(c, null);

                    if (consumerAttribute.PrefetchCount > 0)
                    {
                        c.PrefetchCount = consumerAttribute.PrefetchCount;
                    }

                    if (!String.IsNullOrWhiteSpace(consumerAttribute.ExchangeName))
                    {
                        c.Bind(consumerAttribute.ExchangeName);
                    }

                    if (consumerAttribute.ConcurrenyLimit > 0)
                    {
                        c.UseConcurrencyLimit(consumerAttribute.ConcurrenyLimit);
                    }

                    if (consumerAttribute.ImmediateRetry > 0)
                    {
                        c.UseRetry(retryConfigurator => { retryConfigurator.Immediate(consumerAttribute.ImmediateRetry); });
                    }
                }
            };

            return configuration;
        }
    }
}