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
        private static ILogger _logger;
        private static ISillyDaemon _daemon;

        public SillycoreDaemonBuilder(SillycoreAppBuilder sillycoreAppBuilder, string serviceName)
        {
            _serviceName = serviceName;
            _sillycoreAppBuilder = sillycoreAppBuilder;
        }

        public void Build()
        {
            _sillycoreAppBuilder
                .WithOnStartAction(OnStart)
                .WithOnStopAction(OnStop)
                .UseWebApi(_serviceName)
                .Build();
        }

        private static void OnStart()
        {
            SillycoreApp.Instance.DataStore.Set(Constants.UseShutDownDelay, false);
            _logger = SillycoreApp.Instance.LoggerFactory.CreateLogger<ILogger>();
            _daemon = SillycoreApp.Instance.ServiceProvider.GetService<ISillyDaemon>();

            try
            {
                _daemon.Start().Wait();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"There was a problem while starting service.");
                throw;
            }
        }

        private static void OnStop()
        {
            try
            {
                _daemon.Stop().Wait();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"There was a problem while stopping service.");
                throw;
            }
        }
    }
}