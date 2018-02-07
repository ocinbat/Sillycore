using Microsoft.Extensions.DependencyInjection;

namespace Sillycore.Daemon
{
    public static class SillycoreAppBuilderExtensions
    {
        public static SillycoreDaemonBuilder UseDaemon<TService>(this SillycoreAppBuilder builder, string serviceName)
            where TService : class, ISillyDaemon
        {
            builder.Services.AddSingleton<ISillyDaemon, TService>();

            return new SillycoreDaemonBuilder(builder, serviceName);
        }
    }
}