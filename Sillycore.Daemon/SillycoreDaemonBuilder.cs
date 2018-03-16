using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sillycore.Web;

namespace Sillycore.Daemon
{
    public class SillycoreDaemonBuilder
    {
        private readonly SillycoreAppBuilder _sillycoreAppBuilder;
        private readonly string _serviceName;

        public SillycoreDaemonBuilder(SillycoreAppBuilder sillycoreAppBuilder, string serviceName)
        {
            _serviceName = serviceName;
            _sillycoreAppBuilder = sillycoreAppBuilder;
        }

        public void Build()
        {
            _sillycoreAppBuilder.UseWebApi(_serviceName)
                .WithOnStartAction(OnStart)
                .WithOnStopAction(OnStop)
                .Build();
        }

        private static void OnStart()
        {
            ILogger logger = SillycoreApp.Instance.LoggerFactory.CreateLogger<ILogger>();
            ISillyDaemon daemon = SillycoreApp.Instance.ServiceProvider.GetService<ISillyDaemon>();

            try
            {
                daemon.Start().Wait();
            }
            catch (Exception e)
            {
                logger.LogError(e, $"There was a problem while starting service.");
                throw;
            }
        }

        private static void OnStop()
        {
            ILogger logger = SillycoreApp.Instance.LoggerFactory.CreateLogger<ILogger>();
            ISillyDaemon daemon = SillycoreApp.Instance.ServiceProvider.GetService<ISillyDaemon>();

            try
            {
                daemon.Stop().Wait();
            }
            catch (Exception e)
            {
                logger.LogError(e, $"There was a problem while stopping service.");
                throw;
            }
        }
    }
}