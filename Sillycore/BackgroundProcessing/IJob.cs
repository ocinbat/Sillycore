using System.Threading.Tasks;

namespace Sillycore.BackgroundProcessing
{
    public interface IJob
    {
        Task Run();
    }
}