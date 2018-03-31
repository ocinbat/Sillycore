using System;
using System.Threading.Tasks;
using ConsoleApp.Data;
using ConsoleApp.Helpers;
using Microsoft.Extensions.Configuration;
using Sillycore.BackgroundProcessing;

namespace ConsoleApp
{
    public class TestJob : IJob
    {
        private readonly SomeHelper _helper;
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;

        public TestJob(SomeHelper helper, IConfiguration configuration, DataContext context)
        {
            _helper = helper;
            _configuration = configuration;
            _context = context;
        }

        public async Task Run()
        {
            await Console.Out.WriteLineAsync(DateTime.UtcNow.ToLongDateString());
        }
    }
}