using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Sillycore.Serilog
{
    public static class SillycoreAppBuilderExtensions
    {
        public static SillycoreAppBuilder UseSerilog(this SillycoreAppBuilder builder)
        {
            builder.BeforeBuild(() =>
            {
                IConfiguration configuration = builder.DataStore.Get<IConfiguration>(Constants.Configuration);

                string environment = (configuration["ASPNETCORE_ENVIRONMENT"] ?? "").ToLowerInvariant();
                string applicationName = builder.DataStore.Get<string>("ApplicationName");

                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", applicationName)
                    .Enrich.WithProperty("Server", Environment.MachineName)
                    .Enrich.WithProperty("Environment", environment)
                    .CreateLogger();

                ILoggerFactory loggerFactory = builder.DataStore.Get<ILoggerFactory>(Constants.LoggerFactory);
                loggerFactory.AddSerilog(dispose: true);
            });

            return builder;
        }
    }
}