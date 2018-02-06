using System;
using System.Collections.Generic;
using GreenPipes;
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

        public SillycoreConsumerBuilder<T> RegisterConsumer<T>(string queue) where T : class, IConsumer
        {
            _logger.LogDebug($"Registering consumer:{typeof(T)}");

            var consumerBuilder = new SillycoreConsumerBuilder<T>(this, queue);
            return consumerBuilder;
        }

        internal void AddConsumerConfiguration(ConsumerConfiguration consumerConfiguration)
        {
            _consumerConfigurations.Add((consumerConfiguration));

            _logger.LogDebug($"Consumer:{consumerConfiguration.Type} registered.");
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