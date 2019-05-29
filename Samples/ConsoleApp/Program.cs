using ConsoleApp.Data;
using Sillycore;
using Sillycore.Daemon;
using Sillycore.EntityFramework;
using Sillycore.NLog;
using Sillycore.RabbitMq;
using Sillycore.Serilog;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            SillycoreAppBuilder.Instance
                .UseUtcTimes()
                .UseSerilog()
                .UseDataContext<DataContext>("DataContext")
                .UseRabbitMq()
                .UseDaemon<Service>("ConsoleApp")
                .Build();
        }
    }
}