using Sillycore;
using Sillycore.NLog;
using Sillycore.Web;
using Sillycore.Web.Security;

namespace WebApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            /*
             * To enable authentication & authorization:
             *  1) Comment in the WithAuthentication & WithAuthorization options
             *  2) Comment in the Authorization attribute above the SampleController class declaration
             */

            SillycoreAppBuilder.Instance
                .UseUtcTimes()
                .UseNLog()
                .UseWebApi("WebApplication")
                    .WithSwagger()
                    //.WithAuthentication(new SillycoreAuthenticationOptions("AuthServer"))
                    //.WithAuthorization(new SillycoreAuthorizationOptions()
                    //    .WithPolicy(SillycoreAuthorizationPolicy.Create("defaultPolicy", "lookup")))
                    .Build();
        }
    }
}
