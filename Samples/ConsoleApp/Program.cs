using ConsoleApp.Data;
using Sillycore;
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
                .UseDataContext<DataContext>("DataContext")
                .UseRabbitMq()
                .UseDaemon<Service>("ConsoleApp")
                .Build();
        }
    }
}