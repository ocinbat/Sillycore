using System;
using System.Threading.Tasks;
using Sillycore.BackgroundProcessing;

namespace WebApplication.BackgroundJobs
{
    public class TestJob : IJob
    {
        public async Task Run()
        {
            await Console.Out.WriteLineAsync("Aboooow");
        }
    }
}