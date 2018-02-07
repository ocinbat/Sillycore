using System.Threading.Tasks;
using Sillycore;
using Sillycore.Daemon;

namespace ConsoleApp
{
    public class Service : ISillyDaemon
    {
        public async Task Start()
        {
            SillycoreApp.Instance.BackgroundJobManager.Register<TestJob>(1000);
            await SillycoreApp.Instance.BackgroundJobManager.Start();
        }

        public async Task Stop()
        {
            await SillycoreApp.Instance.BackgroundJobManager.Stop();
        }
    }
}