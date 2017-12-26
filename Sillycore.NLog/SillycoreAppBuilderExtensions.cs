using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Sillycore.NLog
{
    public static class SillycoreAppBuilderExtensions
    {
        public static SillycoreAppBuilder UseNLog(this SillycoreAppBuilder builder)
        {
            builder.AfterBuild(() =>
            {
                IConfiguration configuration = builder.DataStore.Get<IConfiguration>(Constants.Configuration);

                if (!String.IsNullOrWhiteSpace(configuration["NLogConfig"]))
                {
                    File.WriteAllText("nlog.config", configuration["NLogConfig"], Encoding.UTF8);
                }

                string environment = configuration["ASPNETCORE_ENVIRONMENT"].ToLowerInvariant();

                if (environment != "development")
                {
                    if (File.Exists("nlog.config"))
                    {
                        File.Delete("nlog.config");
                    }

                    string environmentFileName = $"nlog.{environment}.config";

                    if (File.Exists(environmentFileName))
                    {
                        File.Move(environmentFileName, "nlog.config");
                    }
                }

                ILoggerFactory loggerFactory = builder.DataStore.Get<ILoggerFactory>(Constants.LoggerFactory);
                loggerFactory.AddNLog();
            });

            return builder;
        }
    }
}