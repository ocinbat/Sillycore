using System;
using System.Collections.Generic;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.DependencyInjection;

namespace Sillycore.RabbitMq
{
    public class SillycoreRabbitMqBuilder
    {
        private readonly SillycoreAppBuilder _sillycoreAppBuilder;
        private readonly string _url;
        private readonly string _username;
        private readonly string _password;

        private readonly List<ConsumerConfiguration> _consumerConfigurations = new List<ConsumerConfiguration>();

        public SillycoreRabbitMqBuilder(SillycoreAppBuilder sillycoreAppBuilder, string url, string username, string password)
        {
            _sillycoreAppBuilder = sillycoreAppBuilder;
            _url = url;
            _username = username;
            _password = password;
        }

        public SillycoreRabbitMqBuilder RegisterConsumer<T>(string queue, ushort? prefetchCount) where T : class, IConsumer
        {
            ConsumerConfiguration configuration = new ConsumerConfiguration();
            configuration.Queue = queue;
            configuration.Type = typeof(T);

            configuration.ConfigureAction = (c) =>
            {
                ICachedConfigurator configurator = new SillycoreConsumerConfigurator<T>();
                configurator.Configure(c, null);
                c.PrefetchCount = prefetchCount ?? 32;
            };

            return this;
        }

        public SillycoreAppBuilder Then()
        {
            _sillycoreAppBuilder.BeforeBuild(() =>
            {
                IBusControl busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    var host = cfg.Host(new Uri(_url), h =>
                    {
                        h.Username(_username);
                        h.Password(_password);
                    });

                    foreach (ConsumerConfiguration consumerConfiguration in _consumerConfigurations)
                    {
                        _sillycoreAppBuilder.Services.AddTransient(consumerConfiguration.Type);
                        cfg.ReceiveEndpoint(host, consumerConfiguration.Queue, consumerConfiguration.ConfigureAction);
                    }

                    cfg.UseExtensionsLogging(_sillycoreAppBuilder.LoggerFactory);
                });

                _sillycoreAppBuilder.Services.AddSingleton(busControl);
                busControl.Start();
            });

            return _sillycoreAppBuilder;
        }
    }
}