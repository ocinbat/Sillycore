using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sillycore.Web;

namespace Sillycore.Daemon
{
    public static class SillycoreAppBuilderExtensions
    {
        public static SillycoreAppBuilder UseDaemon<TService>(this SillycoreAppBuilder builder, string serviceName)
            where TService : class, ISillyDaemon
        {
            builder.Services.AddSingleton<ISillyDaemon, TService>();

            builder.UseWebApi(serviceName)
                .WithOnStartAction(OnStart)
                .WithOnStopAction(OnStop)
                .Build();

            return builder;
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

            Environment.Exit(0);
        }
    }
}