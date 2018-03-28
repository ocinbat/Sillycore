using System;
using System.Threading;
using System.Threading.Tasks;
using ConsoleApp.Helpers;
using Microsoft.Extensions.Configuration;
using Sillycore.BackgroundProcessing;

namespace ConsoleApp
{
    public class TestJob : IJob
    {
        private readonly SomeHelper _helper;
        private readonly IConfiguration _configuration;

        public TestJob(SomeHelper helper, IConfiguration configuration)
        {
            _helper = helper;
            _configuration = configuration;
        }

        public async Task Run()
        {
            await Console.Out.WriteLineAsync(_configuration["TestConfig"]);
        }
    }
}