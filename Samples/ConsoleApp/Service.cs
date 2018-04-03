using System.Threading.Tasks;
using Sillycore;
using Sillycore.BackgroundProcessing;
using Sillycore.Daemon;

namespace ConsoleApp
{
    public class Service : ISillyDaemon
    {
        private readonly BackgroundJobManager _backgroundJobManager;

        public Service(BackgroundJobManager backgroundJobManager)
        {
            _backgroundJobManager = backgroundJobManager;
        }

        public async Task Start()
        {
            _backgroundJobManager.Register<TestJob>("TestJobIntervalInMs");
            await _backgroundJobManager.Start();
        }

        public async Task Stop()
        {
            await _backgroundJobManager.Stop();
        }
    }
}