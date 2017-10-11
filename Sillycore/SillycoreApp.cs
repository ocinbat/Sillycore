using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sillycore.Domain.Abstractions;
using Sillycore.Domain.Objects.DateTimeProviders;

namespace Sillycore
{
    public class SillycoreApp
    {
        public static JsonSerializerSettings JsonSerializerSettings { get; set; }
        public static SillycoreApp Instance { get; set; }

        public InMemoryDataStore DataStore { get; set; }

        public IDateTimeProvider DateTimeProvider => DataStore.Get<IDateTimeProvider>(Constants.DateTimeProvider);
        public ILoggerFactory LoggerFactory => DataStore.Get<ILoggerFactory>(Constants.LoggerFactory);
        public IConfiguration Configuration => DataStore.Get<IConfiguration>(Constants.Configuration);

        public SillycoreApp(InMemoryDataStore dataStore)
        {
            DataStore = dataStore;
        }
    }
}