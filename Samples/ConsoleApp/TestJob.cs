using System;
using System.Threading;
using System.Threading.Tasks;
using ConsoleApp.Helpers;
using Sillycore.BackgroundProcessing;

namespace ConsoleApp
{
    public class TestJob : IJob
    {
        private readonly SomeHelper _helper;

        public TestJob(SomeHelper helper)
        {
            _helper = helper;
        }

        public async Task Run()
        {
            await Console.Out.WriteLineAsync("dsadasda");
        }
    }
}