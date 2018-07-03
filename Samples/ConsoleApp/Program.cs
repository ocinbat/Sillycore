using ConsoleApp.Data;
using Microsoft.Extensions.Logging;
using Sillycore;
using Sillycore.AppMetrics;
using Sillycore.Daemon;
using Sillycore.EntityFramework;
using Sillycore.NLog;
using Sillycore.RabbitMq;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            SillycoreAppBuilder.Instance
                .UseUtcTimes()
                .UseNLog()
                .WithAppMetrics()
                .UseDataContext<DataContext>("DataContext")
                .UseRabbitMq()
                .UseDaemon<Service>("ConsoleApp")
                .Build();
        }
    }
}