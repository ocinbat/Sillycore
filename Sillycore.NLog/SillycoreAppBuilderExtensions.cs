using Microsoft.Extensions.DependencyInjection;
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
                ILoggerFactory loggerFactory = builder.DataStore.Get<ILoggerFactory>(Constants.LoggerFactory);
                loggerFactory.AddNLog();
            });

            return builder;
        }
    }
}