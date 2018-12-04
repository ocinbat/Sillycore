namespace Sillycore.Web.Configuration
{
    public class MetricsElasticSearchOptions
    {
        public bool Enabled { get; set; }
        public string BaseUri { get; set; }
        public string Index { get; set; }
        public int FlushIntervalInSeconds { get; set; }
    }
}
