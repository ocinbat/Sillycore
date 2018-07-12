using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GreenPipes;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sillycore.Extensions;
using Sillycore.RabbitMq.Attributes;
using Sillycore.RabbitMq.Configuration;

namespace Sillycore.RabbitMq
{
    public static class SillycoreAppBuilderExtensions
    {
        private static readonly BusControlProvider BusControlProvider = new BusControlProvider();

        public static SillycoreAppBuilder UseRabbitMq(this SillycoreAppBuilder builder, string configKey = "RabbitMq")
        {
            RabbitMqConfiguration rabbitMqConfiguration = builder.Configuration.GetSection(configKey).Get<RabbitMqConfiguration>();

            if (rabbitMqConfiguration == null)
            {
                throw new ConfigurationException($"No rabbit mq configuration found at section:{configKey}.");
            }

            List<ConsumerConfiguration> consumerConfigurations = new List<ConsumerConfiguration>();

            foreach (TypeInfo typeInfo in Assembly.GetEntryAssembly().DefinedTypes)
            {
                ConsumerAttribute consumerAttribute = typeInfo.GetCustomAttribute<ConsumerAttribute>();

                if (consumerAttribute != null && (String.IsNullOrWhiteSpace(consumerAttribute.RabbitMq) || consumerAttribute.RabbitMq.Equals(configKey)))
                {
                    ConsumerConfiguration consumerConfiguration = CreateConsumerConfigurationForType(typeInfo.AsType(), consumerAttribute);
                    consumerConfigurations.Add(consumerConfiguration);
                }
            }

            builder.Services.AddSingleton<IBusControlProvider>(BusControlProvider);
            builder.Services.TryAddSingleton(sp =>
            {
                if (BusControlProvider.GetBusControls().Count > 1)
                {
                    throw new ConfigurationException($"You cannot resolve IBusControl when there are multiple RabbitMq instances registered to services collection. Instead try to resolve IBusControlProvider and use GetBusControl method with config name you set for your desired RabbitMq instance.");
                }

                return BusControlProvider.GetBusControls().First();
            });

            builder.BeforeBuild(() =>
            {
                IBusControl busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    var host = cfg.Host(new Uri(rabbitMqConfiguration.Url), h =>
                    {
                        h.Username(rabbitMqConfiguration.Username);
                        h.Password(rabbitMqConfiguration.Password);

                        if (rabbitMqConfiguration.Nodes.HasElements())
                        {
                            h.UseCluster(cc => cc.ClusterMembers = rabbitMqConfiguration.Nodes);
                        }
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

                    if (rabbitMqConfiguration.ConcurrencyLimit > 0)
                    {
                        cfg.UseConcurrencyLimit(rabbitMqConfiguration.ConcurrencyLimit);
                    }
                });

                BusControlProvider.AddBusControl(configKey, busControl);
                builder.WhenStart(() => busControl.Start());
                builder.WhenStopped(() => busControl.Stop());
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