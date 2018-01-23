using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sillycore;
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
                .Build();

            _logger = SillycoreApp.Instance.LoggerFactory.CreateLogger<Program>();
            _logger.LogInformation("dsadsada");

            IConfiguration configuration = SillycoreApp.Instance.ServiceProvider.GetService<IConfiguration>();

            Console.Out.WriteLineAsync(SillycoreApp.Instance.Configuration["key"]);
            Console.ReadKey();
        }
    }
}