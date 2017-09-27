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

        private IDateTimeProvider _dateTimeProvider;
        public IDateTimeProvider DateTimeProvider
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

        public SillycoreApp(InMemoryDataStore dataStore)
        {
            DataStore = dataStore;
        }
    }
}