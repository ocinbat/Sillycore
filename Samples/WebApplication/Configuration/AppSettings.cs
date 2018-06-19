using Sillycore.Configuration;

namespace WebApplication.Configuration
{
    [Configuration("AppSettings")]
    public class AppSettings
    {
        public string Test { get; set; }
    }
}