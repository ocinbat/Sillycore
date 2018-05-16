using System;
using System.Threading.Tasks;
using ConsoleApp.Commands;
using ConsoleApp.Data;
using ConsoleApp.Events;
using ConsoleApp.Helpers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Sillycore.BackgroundProcessing;
using Sillycore.RabbitMq;

namespace ConsoleApp
{
    public class TestJob : IJob
    {
        private readonly SomeHelper _helper;
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        private readonly IBusControl _busControl;
        private readonly IBusControlProvider _busControlProvider;

        public TestJob(SomeHelper helper, IConfiguration configuration, DataContext context, IBusControl busControl, IBusControlProvider busControlProvider)
        {
            _helper = helper;
            _configuration = configuration;
            _context = context;
            _busControl = busControl;
            _busControlProvider = busControlProvider;
        }

        public async Task Run()
        {
            string data = Guid.NewGuid().ToString();

            await _busControl.Publish(new SomeEvent()
            {
                Data = data
            });

            await _busControl.Send(new SomeCommand()
            {
                Data = data
            }, "superb_command_queue");
        }
    }
}