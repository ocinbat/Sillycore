using System;
using Microsoft.Extensions.Logging;
using Sillycore;

namespace ConsoleApp
{
    class Program
    {
        static ILogger<Program> _logger;

        static void Main(string[] args)
        {
            SillycoreAppBuilder.Instance
                .UseUtcTimes()
                .Build();

            _logger = SillycoreApp.Instance.LoggerFactory.CreateLogger<Program>();
            _logger.LogInformation("dsadsada");

            Console.Out.WriteLineAsync(SillycoreApp.Instance.Configuration["key"]);
            Console.ReadKey();
        }
    }
}