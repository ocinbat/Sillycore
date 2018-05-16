using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sillycore.Web.HealthCheck;

namespace Sillycore.Web
{
    public class SillycoreWebhostBuilder
    {
        private readonly string[] _args;
        private readonly string _applicationName;
        public readonly SillycoreAppBuilder SillycoreAppBuilder;

        private bool _withIisIntegration = false;
        private Type _sillycoreStartup = null;

        public SillycoreWebhostBuilder(SillycoreAppBuilder sillycoreAppBuilder, string applicationName, string[] args)
        {
            SillycoreAppBuilder = sillycoreAppBuilder;
            _applicationName = applicationName;
            _args = args;
            IWebHostBuilder webhostBuilder = CreateDefaultBuilder(_args);

            SillycoreAppBuilder.DataStore.Set(Constants.ApplicationName, _applicationName);
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

        public SillycoreWebhostBuilder WithSwagger()
        {
            SillycoreAppBuilder.DataStore.Set(Constants.UseSwagger, true);

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

        public SillycoreWebhostBuilder WithStartup(Type startup)
        {
            if (startup != null)
            {
                _sillycoreStartup = startup;
            }

            return this;
        }

        public void Build()
        {
            SillycoreAppBuilder.DataStore.Set(Sillycore.Constants.UseShutDownDelay, true);
            RegisterHealthCheckers();

            SillycoreAppBuilder.BeforeBuild(() =>
            {
                IWebHostBuilder webhostBuilder = SillycoreAppBuilder.DataStore.Get<IWebHostBuilder>(Constants.WebHostBuilder)
                    .UseStartup(_sillycoreStartup == null ? typeof(SillycoreStartup) : _sillycoreStartup);
                SillycoreAppBuilder.DataStore.Set(Constants.WebHostBuilder, webhostBuilder);
            });

            SillycoreApp app = SillycoreAppBuilder.Build();

            IServiceProvider serviceProvider = app.DataStore.Get<IServiceProvider>(Sillycore.Constants.ServiceProvider);
            ILogger<SillycoreWebhostBuilder> logger = serviceProvider.GetService<ILogger<SillycoreWebhostBuilder>>();
            logger.LogInformation($"{_applicationName} started.");

            app.DataStore.Get<IWebHostBuilder>(Constants.WebHostBuilder).Build().Run();
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
            webHostBuilder.UseApplicationInsights();

            if (_withIisIntegration)
            {
                webHostBuilder.UseIISIntegration();
            }

            return webHostBuilder;
        }
    }
}