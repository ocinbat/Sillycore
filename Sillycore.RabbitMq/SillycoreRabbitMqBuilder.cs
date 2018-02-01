using System;
using System.Collections.Generic;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sillycore.RabbitMq
{
    public class SillycoreRabbitMqBuilder
    {
        private readonly SillycoreAppBuilder _sillycoreAppBuilder;
        private readonly string _url;
        private readonly string _username;
        private readonly string _password;
        private readonly ILogger<SillycoreRabbitMqBuilder> _logger;

        private readonly List<ConsumerConfiguration> _consumerConfigurations = new List<ConsumerConfiguration>();

        public SillycoreRabbitMqBuilder(SillycoreAppBuilder sillycoreAppBuilder, string url, string username, string password)
        {
            _sillycoreAppBuilder = sillycoreAppBuilder;
            _url = url;
            _username = username;
            _password = password;
            _logger = _sillycoreAppBuilder.LoggerFactory.CreateLogger<SillycoreRabbitMqBuilder>();
        }

        public SillycoreRabbitMqBuilder RegisterConsumer<T>(string queue, ushort? prefetchCount = null) where T : class, IConsumer
        {
            _logger.LogDebug($"Registering consumer:{typeof(T)}");
            ConsumerConfiguration configuration = new ConsumerConfiguration();
            configuration.Queue = queue;
            configuration.Type = typeof(T);

            configuration.ConfigureAction = (c) =>
            {
                _logger.LogDebug($"Configuring consumer:{typeof(T)}");
                ICachedConfigurator configurator = new SillycoreConsumerConfigurator<T>();
                configurator.Configure(c, null);
                c.PrefetchCount = prefetchCount ?? 32;
                _logger.LogDebug($"Consumer:{typeof(T)} configured.");
            };
            _logger.LogDebug($"Consumer:{typeof(T)} registered.");

            _consumerConfigurations.Add(configuration);

            return this;
        }

        public SillycoreAppBuilder Then()
        {
            _sillycoreAppBuilder.BeforeBuild(() =>
            {
                IBusControl busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    _logger.LogDebug($"Configuring bus.");
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
                    _logger.LogDebug($"Bus configured.");
                });

                _sillycoreAppBuilder.Services.AddSingleton(busControl);
                busControl.Start();
                _logger.LogDebug($"Bus started.");
            });

            return _sillycoreAppBuilder;
        }
    }
}