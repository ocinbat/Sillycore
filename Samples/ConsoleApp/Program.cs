using System;
using ConsoleApp.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sillycore;
using Sillycore.Daemon;
using Sillycore.EntityFramework;
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
                .UseDataContext<DataContext>("DataContext")
                .UseDaemon<Service>("ConsoleApp")
                .Build();
        }
    }
}