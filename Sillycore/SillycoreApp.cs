using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sillycore.Domain.Abstractions;
using Sillycore.Domain.Objects.DateTimeProviders;

namespace Sillycore
{
    public class SillycoreApp
    {
        public static JsonSerializerSettings JsonSerializerSettings { get; set; }

        public static InMemoryDataStore DataStore { get; set; }

        private static IDateTimeProvider _dateTimeProvider;
        public static IDateTimeProvider DateTimeProvider
        {
            get
            {
                if (_dateTimeProvider == null)
                {
                    _dateTimeProvider = DataStore.GetData<IDateTimeProvider>(Constants.DateTimeProvider);

                    if (_dateTimeProvider == null)
                    {
                        _dateTimeProvider = new UtcDateTimeProvider();
                    }
                }

                return _dateTimeProvider;
            }
        }

        public static ILoggerFactory LoggerFactory => DataStore.GetData<ILoggerFactory>(Constants.LoggerFactory);

        public static void Initialize(InMemoryDataStore dataStore)
        {
            DataStore = dataStore;
        }
    }
}