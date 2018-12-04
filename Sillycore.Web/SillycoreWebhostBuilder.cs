using System;
using System.IO;
using System.Linq;
using System.Reflection;
using App.Metrics.AspNetCore;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sillycore.Web.HealthCheck;

namespace Sillycore.Web
{
    public class SillycoreWebhostBuilder
    {
        private readonly string[] _args;
        public readonly string ApplicationName;
        public readonly SillycoreAppBuilder SillycoreAppBuilder;

        private bool _withIisIntegration = false;

        public SillycoreWebhostBuilder(SillycoreAppBuilder sillycoreAppBuilder, string applicationName, string[] args)
        {
            TelemetryConfiguration.Active.DisableTelemetry = true;
            SillycoreAppBuilder = sillycoreAppBuilder;
            ApplicationName = applicationName;
            _args = args;
            IWebHostBuilder webhostBuilder = CreateDefaultBuilder(_args);

            SillycoreAppBuilder.DataStore.Set(Constants.ApplicationName, ApplicationName);
            SillycoreAppBuilder.DataStore.Set(Constants.WebHostBuilder, webhostBuilder);
            SillycoreAppBuilder.DataStore.Set(Constants.IsShuttingDown, false);
            SillycoreAppBuilder.DataStore.Set(Constants.UseSwagger, false);
            SillycoreAppBuilder.DataStore.Set(Constants.RequiresAuthentication, false);
        }

        public SillycoreWebhostBuilder WithUrl(string rootUrl)
        {
            if (!String.IsNullOrEmpty(rootUrl))
            {
                SillycoreAppBuilder.DataStore.Set(Constants.ApiRootUrl, rootUrl.TrimEnd('/'));
            }

            return this;
        }

        public SillycoreWebhostBuilder WithSwagger(bool redirectRootToSwagger = true)
        {
            SillycoreAppBuilder.DataStore.Set(Constants.UseSwagger, true);
            SillycoreAppBuilder.DataStore.Set(Constants.RedirectRootToSwagger, redirectRootToSwagger);

            return this;
        }

        public SillycoreWebhostBuilder WithIisIntegration()
        {
            _withIisIntegration = true;
            return this;
        }

        public SillycoreAuthenticationBuilder WithAuthentication()
        {
            SillycoreAppBuilder.DataStore.Set(Constants.RequiresAuthentication, true);
            var sillycoreAuthenticationBuilder = new SillycoreAuthenticationBuilder(this, SillycoreAppBuilder.DataStore);
            return sillycoreAuthenticationBuilder;
        }

        public void Build()
        {
            SillycoreAppBuilder.DataStore.Set(Sillycore.Constants.UseShutDownDelay, true);
            RegisterHealthCheckers();

            IWebHost webHost = null;

            SillycoreAppBuilder.BeforeBuild(() =>
            {
                IWebHostBuilder webhostBuilder = SillycoreAppBuilder.DataStore.Get<IWebHostBuilder>(Constants.WebHostBuilder)
                    .UseStartup(typeof(SillycoreStartup));

                webHost = webhostBuilder
                    .Build();
            });

            SillycoreApp app = SillycoreAppBuilder.Build();
            ILogger<SillycoreWebhostBuilder> logger = app.ServiceProvider.GetService<ILoggerFactory>().CreateLogger<SillycoreWebhostBuilder>();
            logger.LogInformation($"{ApplicationName} started.");

            webHost.Run();
        }

        private void RegisterHealthCheckers()
        {
            HealthCheckerContainer container = SillycoreAppBuilder.DataStore.Get<HealthCheckerContainer>(Constants.HealthCheckerContainerDataKey);

            if (container == null)
            {
                container = new HealthCheckerContainer();
            }

            Assembly ass = Assembly.GetEntryAssembly();

            foreach (TypeInfo ti in ass.DefinedTypes)
            {
                if (ti.ImplementedInterfaces.Contains(typeof(IHealthChecker)))
                {
                    container.AddHealthChecker(ti);

                    SillycoreAppBuilder.Services.AddTransient(ti);
                }
            }

            SillycoreAppBuilder.DataStore.Set(Constants.HealthCheckerContainerDataKey, container);
        }

        public IWebHostBuilder CreateDefaultBuilder(string[] args)
        {
            var webHostBuilder = new WebHostBuilder();

            webHostBuilder.UseKestrel();
            webHostBuilder.UseContentRoot(Directory.GetCurrentDirectory());
            webHostBuilder.UseConfiguration(SillycoreAppBuilder.Instance.Configuration);

            if (_withIisIntegration)
            {
                webHostBuilder.UseIISIntegration();
            }

            return webHostBuilder;
        }
    }
}