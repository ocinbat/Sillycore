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

                string environment = (configuration["ASPNETCORE_ENVIRONMENT"] ?? "").ToLowerInvariant();

                if (!String.IsNullOrWhiteSpace(environment))
                {
                    if (environment != "development")
                    {
                        string environmentFileName = $"nlog.{environment}.config";

                        if (File.Exists(environmentFileName))
                        {
                            if (File.Exists("nlog.config"))
                            {
                                File.Delete("nlog.config");
                            }

                            File.Move(environmentFileName, "nlog.config");
                        }
                    }
                }

                ILoggerFactory loggerFactory = builder.DataStore.Get<ILoggerFactory>(Constants.LoggerFactory);
                loggerFactory.AddNLog();
            });

            return builder;
        }
    }
}