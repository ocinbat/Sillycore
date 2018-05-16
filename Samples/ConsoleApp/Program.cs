using ConsoleApp.Data;
using Microsoft.Extensions.Logging;
using Sillycore;
using Sillycore.Daemon;
using Sillycore.EntityFramework;
using Sillycore.NLog;
using Sillycore.RabbitMq;

namespace ConsoleApp
{
    class Program
    {
        static ILogger<Program> _logger;

        static void Main(string[] args)
        {
            SillycoreAppBuilder.Instance
                .UseConfigServer("https://config-server.trendyol.com", "Silkroad.ReservationCleaner", 10000)
                .UseUtcTimes()
                .UseNLog()
                .UseDataContext<DataContext>("DataContext")
                .UseRabbitMq()
                .UseDaemon<Service>("ConsoleApp")
                .Build();
        }
    }
}