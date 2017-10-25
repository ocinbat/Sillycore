using System;
using System.IO;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sillycore.Web
{
    public class SillycoreWebhostBuilder
    {
        private readonly string[] _args;
        private readonly string _applicationName;
        private readonly SillycoreAppBuilder _sillycoreAppBuilder;

        private bool _withIisIntegration = false;

        public SillycoreWebhostBuilder(SillycoreAppBuilder sillycoreAppBuilder, string applicationName, string[] args)
        {
            _sillycoreAppBuilder = sillycoreAppBuilder;
            _applicationName = applicationName;
            _args = args;
        }

        public SillycoreWebhostBuilder WithUrl(string rootUrl)
        {
            if (!String.IsNullOrEmpty(rootUrl))
            {
                _sillycoreAppBuilder.DataStore.Set(Constants.ApiRootUrl, rootUrl.TrimEnd('/'));
            }

            return this;
        }

        public SillycoreWebhostBuilder WithIisIntegration()
        {
            _withIisIntegration = true;
            return this;
        }

        public SillycoreWebhostBuilder WithLocalSsl(string certificatePath, string password)
        {
            _sillycoreAppBuilder.DataStore.Set(Constants.WithLocalSsl, true);
            _sillycoreAppBuilder.DataStore.Set(Constants.CertificatePath, certificatePath);
            _sillycoreAppBuilder.DataStore.Set(Constants.CertificatePassword, password);
            return this;
        }

        public void Build()
        {
            _sillycoreAppBuilder.DataStore.Set(Constants.ApplicationName, _applicationName);

            _sillycoreAppBuilder.BeforeBuild(() =>
            {
                IWebHost webhost = CreateDefaultBuilder(_args)
                    .UseStartup<Startup>()
                    .Build();

                _sillycoreAppBuilder.DataStore.Set(Constants.WebHost, webhost);
            });

            SillycoreApp app = _sillycoreAppBuilder.Build();

            ServiceProvider serviceProvider = app.DataStore.Get<ServiceProvider>(Sillycore.Constants.ServiceProvider);
            ILogger<SillycoreWebhostBuilder> logger = serviceProvider.GetService<ILogger<SillycoreWebhostBuilder>>();
            logger.LogInformation($"{_applicationName} started.");

            app.DataStore.Get<IWebHost>(Constants.WebHost).Run();
        }

        public IWebHostBuilder CreateDefaultBuilder(string[] args)
        {
            var webHostBuilder = new WebHostBuilder();

            if (_sillycoreAppBuilder.DataStore.Get<bool>(Constants.WithLocalSsl))
            {
                webHostBuilder.UseKestrel(options =>
                {
                    options.AddServerHeader = false;
                    options.Listen(IPAddress.Any, 443, listenOptions =>
                    {
                        listenOptions.UseHttps(_sillycoreAppBuilder.DataStore.Get<string>(Constants.CertificatePath), _sillycoreAppBuilder.DataStore.Get<string>(Constants.CertificatePassword));
                    });
                });
            }
            else
            {
                webHostBuilder.UseKestrel();
            }

            webHostBuilder.UseContentRoot(Directory.GetCurrentDirectory()).ConfigureAppConfiguration((Action<WebHostBuilderContext, IConfigurationBuilder>)((hostingContext, config) =>
            {
                IHostingEnvironment hostingEnvironment = hostingContext.HostingEnvironment;
                config.AddJsonFile("appsettings.json", true, true).AddJsonFile(string.Format("appsettings.{0}.json", (object)hostingEnvironment.EnvironmentName), true, true);
                if (hostingEnvironment.IsDevelopment())
                {
                    Assembly assembly = Assembly.Load(new AssemblyName(hostingEnvironment.ApplicationName));
                    if (assembly != (Assembly)null)
                        config.AddUserSecrets(assembly, true);
                }
                config.AddEnvironmentVariables();
                if (args == null)
                    return;
                config.AddCommandLine(args);
            })).ConfigureLogging((Action<WebHostBuilderContext, ILoggingBuilder>)((hostingContext, logging) =>
            {
                logging.AddConfiguration((IConfiguration)hostingContext.Configuration.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();
            }));

            if (_withIisIntegration)
            {
                webHostBuilder.UseIISIntegration();
            }

            webHostBuilder.UseDefaultServiceProvider((Action<WebHostBuilderContext, ServiceProviderOptions>)((context, options) => options.ValidateScopes = context.HostingEnvironment.IsDevelopment()));

            return webHostBuilder;
        }
    }
}