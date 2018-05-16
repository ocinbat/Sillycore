using System.Threading.Tasks;
using MassTransit;
using Sillycore.BackgroundProcessing;
using Sillycore.Daemon;

namespace ConsoleApp
{
    public class Service : ISillyDaemon
    {
        private readonly IBusControl _busControl;
        private readonly BackgroundJobManager _backgroundJobManager;

        public Service(BackgroundJobManager backgroundJobManager, IBusControl busControl)
        {
            _backgroundJobManager = backgroundJobManager;
            _busControl = busControl;
        }

        public async Task Start()
        {
            _backgroundJobManager.Register<TestJob>("TestJobIntervalInMs");
            await _backgroundJobManager.Start();
        }

        public async Task Stop()
        {
            await _backgroundJobManager.Stop();
            await _busControl.StopAsync();
        }
    }
}