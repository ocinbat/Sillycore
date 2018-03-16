using System.Threading.Tasks;
using Sillycore.Web.HealthCheck;

namespace WebApplication.HealthCheckers
{
    public class DatabaseHealthChecker : IHealthChecker
    {
        public string Key => "sql";
        public bool IsCritical => true;
        public Task<bool> CheckHealth()
        {
            return Task.FromResult(true);
        }
    }
}