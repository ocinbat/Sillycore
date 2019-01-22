using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sillycore.Web.Abstractions;

namespace Sillycore.Web.ApplicationLifetime
{
    public class ApplicationLifetimeConfigurator : IApplicationConfigurator
    {
        public int Order => 10800;
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            IApplicationLifetime applicationLifetime = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();

            applicationLifetime.ApplicationStopping.Register(() =>
            {
                if (SillycoreApp.Instance.DataStore.Get<bool>(Sillycore.Constants.UseShutDownDelay))
                {
                    string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                    if (!String.IsNullOrWhiteSpace(environment) && environment.ToLowerInvariant() != "development")
                    {
                        SillycoreApp.Instance.DataStore.Set(Constants.IsShuttingDown, true);
                        Thread.Sleep(30000);
                    }
                }

                SillycoreApp.Instance.Stopping();
            });

            applicationLifetime.ApplicationStopped.Register(() =>
            {
                SillycoreApp.Instance.Stopped();
            });
        }
    }
}