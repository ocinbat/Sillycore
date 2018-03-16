using System.Threading.Tasks;

namespace Sillycore.Web.HealthCheck
{
    public interface IHealthChecker
    {
        string Key { get; }

        bool IsCritical { get; }

        Task<bool> CheckHealth();
    }
}