using System;
using System.Threading;
using System.Threading.Tasks;
using Sillycore.BackgroundProcessing;

namespace ConsoleApp
{
    public class TestJob : IJob
    {
        public async Task Run()
        {
            await Console.Out.WriteLineAsync("dsadasda");
        }
    }
}