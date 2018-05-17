using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Sillycore.BackgroundProcessing;
using Sillycore.Daemon;
using Sillycore.RabbitMq;

namespace ConsoleApp
{
    public class Service : ISillyDaemon
    {
        private readonly IBusControl _busControl;
        private readonly BackgroundJobManager _backgroundJobManager;

        public Service(BackgroundJobManager backgroundJobManager, IBusControlProvider busControlProvider)
        {
            _backgroundJobManager = backgroundJobManager;
            _busControl = busControlProvider.GetBusControls().First();
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