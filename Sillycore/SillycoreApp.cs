using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Sillycore.BackgroundProcessing;
using Sillycore.Domain.Abstractions;

namespace Sillycore
{
    public class SillycoreApp
    {
        public static JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Converters = new List<JsonConverter> { new StringEnumConverter { CamelCaseText = true } }
        };

        public static SillycoreApp Instance { get; set; }

        public InMemoryDataStore DataStore { get; set; }

        public IDateTimeProvider DateTimeProvider => DataStore.Get<IDateTimeProvider>(Constants.DateTimeProvider);
        public ILoggerFactory LoggerFactory => DataStore.Get<ILoggerFactory>(Constants.LoggerFactory);
        public IConfiguration Configuration => DataStore.Get<IConfiguration>(Constants.Configuration);
        public IServiceProvider ServiceProvider => DataStore.Get<IServiceProvider>(Constants.ServiceProvider);
        public BackgroundJobManager BackgroundJobManager => ServiceProvider.GetService<BackgroundJobManager>();

        public SillycoreApp(InMemoryDataStore dataStore)
        {
            DataStore = dataStore;
        }

        public void Started()
        {
            foreach (Action action in DataStore.Get<List<Action>>(Constants.OnStartActions))
            {
                action.Invoke();
            }
        }

        public void Stopping()
        {
            foreach (Action action in DataStore.Get<List<Action>>(Constants.OnStopActions))
            {
                action.Invoke();
            }
        }

        public void Stopped()
        {
            foreach (Action action in DataStore.Get<List<Action>>(Constants.OnStoppedActions))
            {
                action.Invoke();
            }
        }
    }
}