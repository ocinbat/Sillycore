using Sillycore;
using Sillycore.NLog;
using Sillycore.Web;

namespace WebApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SillycoreAppBuilder.Instance
                .UseUtcTimes()
                .UseNLog()
                .UseWebApi("WebApplication")
                    .WithSwagger()
                    .Build();
        }
    }
}
