using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Sillycore.Domain.Abstractions;
using Sillycore.Domain.Objects.DateTimeProviders;

namespace Sillycore
{
    public class SillycoreAppBuilder : ISillycoreAppBuilder
    {
        private static SillycoreAppBuilder _appBuilder;
        public static SillycoreAppBuilder Instance => _appBuilder ?? (_appBuilder = new SillycoreAppBuilder());

        private readonly List<Action> _beforeBuildTasks = new List<Action>();
        private readonly List<Action> _afterBuildTasks = new List<Action>();

        public InMemoryDataStore DataStore = new InMemoryDataStore();
        public IServiceCollection Services = new ServiceCollection();

        public SillycoreApp Build()
        {
            SetGlobalJsonSerializerSettings();
            InitializeLogger();

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
            DataStore.SetData(Constants.DateTimeProvider, new LocalDateTimeProvider());

            return this;
        }

        public SillycoreAppBuilder UseUtcTimes()
        {
            DataStore.SetData(Constants.DateTimeProvider, new UtcDateTimeProvider());

            return this;
        }

        public SillycoreAppBuilder ConfigureServices(Action<IServiceCollection> action)
        {
            action.Invoke(Services);

            return this;
        }

        private void SetGlobalJsonSerializerSettings()
        {
            IDateTimeProvider dateTimeProvider = DataStore.GetData<IDateTimeProvider>(Constants.DateTimeProvider);

            if (dateTimeProvider != null && dateTimeProvider.Kind == DateTimeKind.Local)
            {
                SillycoreApp.JsonSerializerSettings = GetJsonSerializerSettings(DateTimeZoneHandling.Local);
            }
            else if (dateTimeProvider != null && dateTimeProvider.Kind == DateTimeKind.Utc)
            {
                SillycoreApp.JsonSerializerSettings = GetJsonSerializerSettings(DateTimeZoneHandling.Utc);
            }
            else
            {
                SillycoreApp.JsonSerializerSettings = GetJsonSerializerSettings();
            }
        }

        private JsonSerializerSettings GetJsonSerializerSettings(DateTimeZoneHandling dateTimeZoneHandling = DateTimeZoneHandling.Utc)
        {
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DateTimeZoneHandling = dateTimeZoneHandling
            };

            settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
            return settings;
        }

        private void BuildServiceProvider()
        {
            if (DataStore.GetData(Constants.ServiceProvider) == null)
            {
                ServiceProvider serviceProvider = Services.BuildServiceProvider();
                DataStore.SetData(Constants.ServiceProvider, serviceProvider);
            }
        }

        private void InitializeLogger()
        {
            Services.AddLogging(lb =>
            {
                lb.SetMinimumLevel(LogLevel.Trace);
            });
        }
    }
}