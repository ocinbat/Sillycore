using System;
using System.Threading.Tasks;
using ConsoleApp.Data;
using ConsoleApp.Events;
using ConsoleApp.Helpers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Sillycore.BackgroundProcessing;

namespace ConsoleApp
{
    public class TestJob : IJob
    {
        private readonly SomeHelper _helper;
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        private readonly IBusControl _busControl;

        public TestJob(SomeHelper helper, IConfiguration configuration, DataContext context, IBusControl busControl)
        {
            _helper = helper;
            _configuration = configuration;
            _context = context;
            _busControl = busControl;
        }

        public async Task Run()
        {
            string data = Guid.NewGuid().ToString();
            await _busControl.Publish(message: new SomeEvent()
            {
                Data = data
            });

            await Console.Out.WriteLineAsync($"Event:{data} published.");
        }
    }
}