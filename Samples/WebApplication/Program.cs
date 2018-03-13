using Sillycore;
using Sillycore.NLog;
using Sillycore.Web;
using Sillycore.Web.Security;
using WebApplication.HealthCheckers;

namespace WebApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            /*
             * To enable authentication & authorization:
             *  1) Comment in the WithAuthentication and its following options
             *  2) Comment in the Authorization attribute above the SampleController class declaration
             */

            SillycoreAppBuilder.Instance
                .UseUtcTimes()
                .UseNLog()
                .UseWebApi("WebApplication")
                    .WithHealthChecker<DatabaseHealthChecker>()
                    .WithSwagger()
                        /* .WithAuthentication()
                        .As("AuthServer")
                        .WithPolicy("defaultPolicy", "lookup")
                        .Then() */
                    .Build();
        }
    }
}
