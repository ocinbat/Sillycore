using Microsoft.Extensions.Logging;
using Sillycore;
using Sillycore.Daemon;
using Sillycore.NLog;

namespace ConsoleApp
{
    class Program
    {
        static ILogger<Program> _logger;

        static void Main(string[] args)
        {
            SillycoreAppBuilder.Instance
                .UseUtcTimes()
                .UseNLog()
                .UseDaemon<Service>("ConsoleApp")
                .Build();

            _logger = SillycoreApp.Instance.LoggerFactory.CreateLogger<Program>();
        }
    }
}