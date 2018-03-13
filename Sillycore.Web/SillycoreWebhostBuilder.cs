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
        private readonly SillycoreAppBuilder _sillycoreAppBuilder;

        private bool _withIisIntegration = false;
        private Type _sillycoreStartup = null;

        public SillycoreWebhostBuilder(SillycoreAppBuilder sillycoreAppBuilder, string applicationName, string[] args)
        {
            _sillycoreAppBuilder = sillycoreAppBuilder;
            _applicationName = applicationName;
            _args = args;

            _sillycoreAppBuilder.DataStore.Set(Constants.IsShuttingDown, false);
            _sillycoreAppBuilder.DataStore.Set(Constants.UseSwagger, false);
            _sillycoreAppBuilder.DataStore.Set(Constants.RequiresAuthentication, false);
            _sillycoreAppBuilder.DataStore.Set(Constants.OnStartActions, new List<Action>());
            _sillycoreAppBuilder.DataStore.Set(Constants.OnStopActions, new List<Action>());
        }

        public SillycoreWebhostBuilder WithUrl(string rootUrl)
        {
            if (!String.IsNullOrEmpty(rootUrl))
            {
                _sillycoreAppBuilder.DataStore.Set(Constants.ApiRootUrl, rootUrl.TrimEnd('/'));
            }

            return this;
        }

        public SillycoreWebhostBuilder WithOnStartAction(Action action)
        {
            _sillycoreAppBuilder.DataStore.Get<List<Action>>(Constants.OnStartActions).Add(action);

            return this;
        }

        public SillycoreWebhostBuilder WithOnStopAction(Action action)
        {
            _sillycoreAppBuilder.DataStore.Get<List<Action>>(Constants.OnStopActions).Add(action);

            return this;
        }

        public SillycoreWebhostBuilder WithSwagger()
        {
            _sillycoreAppBuilder.DataStore.Set(Constants.UseSwagger, true);

            return this;
        }

        public SillycoreWebhostBuilder WithIisIntegration()
        {
            _withIisIntegration = true;
            return this;
        }

        public SillycoreAuthenticationBuilder WithAuthentication()
        {
            _sillycoreAppBuilder.DataStore.Set(Constants.RequiresAuthentication, true);
            var sillycoreAuthenticationBuilder = new SillycoreAuthenticationBuilder(this, _sillycoreAppBuilder.DataStore);
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
            RegisterHealthCheckers();

            _sillycoreAppBuilder.DataStore.Set(Constants.ApplicationName, _applicationName);

            _sillycoreAppBuilder.BeforeBuild(() =>
            {
                IWebHost webhost = CreateDefaultBuilder(_args)
                    .UseStartup(_sillycoreStartup == null ? typeof(SillycoreStartup) : _sillycoreStartup)
                    .Build();

                _sillycoreAppBuilder.DataStore.Set(Constants.WebHost, webhost);
            });

            SillycoreApp app = _sillycoreAppBuilder.Build();

            ServiceProvider serviceProvider = app.DataStore.Get<ServiceProvider>(Sillycore.Constants.ServiceProvider);
            ILogger<SillycoreWebhostBuilder> logger = serviceProvider.GetService<ILogger<SillycoreWebhostBuilder>>();
            logger.LogInformation($"{_applicationName} started.");

            app.DataStore.Get<IWebHost>(Constants.WebHost).Run();
        }

        private void RegisterHealthCheckers()
        {
            HealthCheckerContainer container = _sillycoreAppBuilder.DataStore.Get<HealthCheckerContainer>(Constants.HealthCheckerContainerDataKey);

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

                    _sillycoreAppBuilder.Services.AddTransient(ti);
                    _sillycoreAppBuilder.DataStore.Set(Constants.HealthCheckerContainerDataKey, container);
                }
            }
        }

        public IWebHostBuilder CreateDefaultBuilder(string[] args)
        {
            var webHostBuilder = new WebHostBuilder();

            webHostBuilder.UseKestrel();
            webHostBuilder.UseContentRoot(Directory.GetCurrentDirectory());

            if (_withIisIntegration)
            {
                webHostBuilder.UseIISIntegration();
            }

            return webHostBuilder;
        }
    }
}