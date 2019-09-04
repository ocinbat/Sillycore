﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using Anetta.Extensions;
using Anetta.ServiceConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sillycore.BackgroundProcessing;
using Sillycore.Domain.Abstractions;
using Sillycore.Domain.Objects.DateTimeProviders;

namespace Sillycore
{
    public class SillycoreAppBuilder : ISillycoreAppBuilder
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private static SillycoreAppBuilder _appBuilder;
        public static SillycoreAppBuilder Instance => _appBuilder ?? (_appBuilder = new SillycoreAppBuilder());

        private readonly List<Action> _beforeBuildTasks = new List<Action>();
        private readonly List<Action> _afterBuildTasks = new List<Action>();

        public InMemoryDataStore DataStore = new InMemoryDataStore();
        public IServiceCollection Services = new ServiceCollection();
        public IConfigurationRoot Configuration { get; private set; }
        public ILoggerFactory LoggerFactory { get; private set; }

        internal SillycoreAppBuilder()
        {
            DataStore.Set(Constants.OnStartActions, new List<Action>());
            DataStore.Set(Constants.OnStopActions, new List<Action>());
            DataStore.Set(Constants.OnStoppedActions, new List<Action>());
            InitializeConfiguration();
            InitializeLogger();
            InitializeDateTimeProvider();
            InitializeBackgroundJobManager();
        }

        public SillycoreApp Build()
        {
            Services.AddAnnotations();
            Services.AddServiceConfigurators(Configuration);

            foreach (var task in _beforeBuildTasks)
            {
                task.Invoke();
            }

            BuildServiceProvider();
            SillycoreApp.Instance = new SillycoreApp(DataStore);

            foreach (var task in _afterBuildTasks)
            {
                task.Invoke();
            }

            SillycoreApp.Instance.Started();

            return SillycoreApp.Instance;
        }

        public void BeforeBuild(Action action)
        {
            _beforeBuildTasks.Add(action);
        }

        public void AfterBuild(Action action)
        {
            _afterBuildTasks.Add(action);
        }

        public SillycoreAppBuilder UseLocalTimes()
        {
            SillycoreApp.JsonSerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            DataStore.Delete(Constants.DateTimeProvider);
            DataStore.Set(Constants.DateTimeProvider, new LocalDateTimeProvider());
            Services.TryAddSingleton(DataStore.Get<IDateTimeProvider>(Constants.DateTimeProvider));

            return this;
        }

        public SillycoreAppBuilder UseUtcTimes()
        {
            DataStore.Delete(Constants.DateTimeProvider);
            DataStore.Set(Constants.DateTimeProvider, new UtcDateTimeProvider());
            Services.TryAddSingleton(DataStore.Get<IDateTimeProvider>(Constants.DateTimeProvider));

            return this;
        }

        public SillycoreAppBuilder UseConfigServer(string configServerAddress, string appName, int defaultReloadTimeInMiliseconds = 180000)
        {
            string environment = Configuration["ASPNETCORE_ENVIRONMENT"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "development";

            if (environment.ToLowerInvariant() == "development" || environment.ToLowerInvariant() == "ci")
            {
                return this;
            }

            DataStore.Set(Constants.ConfigServerAddress, configServerAddress);
            DataStore.Set(Constants.ConfigServerAppName, appName);
            DataStore.Set(Constants.ConfigServerReloadTimeInMiliseconds, defaultReloadTimeInMiliseconds);
            DataStore.Set(Constants.ConfigServerReloadTimer, new Timer(ReloadConfigurationFromConfigServer, null, defaultReloadTimeInMiliseconds, defaultReloadTimeInMiliseconds));

            ReloadConfigurationFromConfigServer(null);

            return this;
        }

        public SillycoreAppBuilder WhenStart(Action action)
        {
            DataStore.Get<List<Action>>(Constants.OnStartActions).Add(action);

            return this;
        }

        public SillycoreAppBuilder WhenStopping(Action action)
        {
            DataStore.Get<List<Action>>(Constants.OnStopActions).Add(action);

            return this;
        }

        public SillycoreAppBuilder WhenStopped(Action action)
        {
            DataStore.Get<List<Action>>(Constants.OnStoppedActions).Add(action);

            return this;
        }

        private void BuildServiceProvider()
        {
            if (DataStore.Get(Constants.ServiceProvider) == null)
            {
                Services.AddAnnotations();
                IServiceProvider serviceProvider = Services.BuildServiceProvider();
                DataStore.Set(Constants.ServiceProvider, serviceProvider);
            }
        }

        private void InitializeLogger()
        {
            IConfigureOptions<LoggerFilterOptions> options = new ConfigureOptions<LoggerFilterOptions>(filterOptions =>
            {
                filterOptions.MinLevel = LogLevel.Trace;
            });

            var loggerFactory = new LoggerFactory();
            DataStore.Set(Constants.LoggerFactory, loggerFactory);
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            LoggerFactory = loggerFactory;

            Services.AddOptions();
            Services.TryAdd(ServiceDescriptor.Singleton<ILoggerFactory>(loggerFactory));
            Services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));
            Services.TryAddEnumerable(ServiceDescriptor.Singleton(options));
        }

        private void InitializeConfiguration()
        {
            string baseDirectory = Directory.GetCurrentDirectory();
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (String.IsNullOrWhiteSpace(environment))
            {
                environment = "development";
            }

            Configuration = new ConfigurationBuilder()
                .SetBasePath(baseDirectory)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environment.ToLowerInvariant()}.json", true, true)
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.config-server.json", true, true)
                .Build();

            Services.TryAdd(ServiceDescriptor.Singleton(Configuration));
            Services.TryAdd(ServiceDescriptor.Singleton<IConfiguration>(Configuration));

            DataStore.Set(Constants.Configuration, Configuration);
        }

        private void InitializeDateTimeProvider()
        {
            DataStore.Set(Constants.DateTimeProvider, new UtcDateTimeProvider());
        }

        private void InitializeBackgroundJobManager()
        {
            Assembly ass = Assembly.GetEntryAssembly();

            foreach (TypeInfo ti in ass.DefinedTypes)
            {
                if (ti.ImplementedInterfaces.Contains(typeof(IJob)))
                {
                    Services.AddTransient(ti);
                }
            }

            Services.AddSingleton<BackgroundJobManager>();
        }

        private void ReloadConfigurationFromConfigServer(object state)
        {
            Timer timer = DataStore.Get<Timer>(Constants.ConfigServerReloadTimer);
            timer.Change(-1, -1);

            string configServerAddress = DataStore.Get<string>(Constants.ConfigServerAddress);
            string appName = DataStore.Get<string>(Constants.ConfigServerAppName);
            string environment = Configuration["ASPNETCORE_ENVIRONMENT"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "development";
            string url = $"{configServerAddress?.TrimEnd('/')}/{appName}/{environment.ToLowerInvariant()}/master/config.json";

            try
            {
                string baseDirectory = Directory.GetCurrentDirectory();
                string appsettingsConfigServerPath = Path.Combine(baseDirectory, "appsettings.config-server.json");

                File.WriteAllText(appsettingsConfigServerPath, HttpClient.GetStringAsync(url).Result);
                Configuration.Reload();

                DataStore.Set(Constants.ConfigServerFirstLoadSucceeded, true);
            }
            catch (Exception e)
            {
                ILogger<SillycoreAppBuilder> logger = LoggerFactory.CreateLogger<SillycoreAppBuilder>();
                logger.LogError(e, $"There was a problem while loading config from config server:{url}");

                if (!DataStore.Get<bool>(Constants.ConfigServerFirstLoadSucceeded))
                {
                    throw new ApplicationException($"There was a problem while loading config from config server:{url}", e);
                }
            }

            int reloadTime = DataStore.Get<int>(Constants.ConfigServerReloadTimeInMiliseconds);
            timer.Change(reloadTime, reloadTime);
        }
    }
}