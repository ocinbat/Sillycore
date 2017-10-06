using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Extensions.Logging;

namespace Sillycore.NLog
{
    public static class SillycoreAppBuilderExtensions
    {
        public static SillycoreAppBuilder UseNLog(this SillycoreAppBuilder builder)
        {
            builder.AfterBuild(() =>
            {
                ILoggerFactory loggerFactory = builder.DataStore.GetData<ServiceProvider>(Constants.ServiceProvider).GetService<ILoggerFactory>();
                loggerFactory.AddNLog();
                builder.DataStore.SetData(Constants.LoggerFactory, loggerFactory);
            });

            return builder;
        }
    }
}