using System.Threading.Tasks;

namespace Sillycore.Daemon
{
    public interface ISillyDaemon
    {
        Task Start();

        Task Stop();
    }
}