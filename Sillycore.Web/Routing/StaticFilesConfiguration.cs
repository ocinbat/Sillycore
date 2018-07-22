using Sillycore.Configuration;

namespace Sillycore.Web.Routing
{
    [Configuration("Sillycore:Web:StaticFiles")]
    public class StaticFilesConfiguration
    {
        public bool ServeUnknownFileTypes { get; set; } = false;
    }
}