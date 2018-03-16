using System.Threading.Tasks;
using Sillycore.Web.HealthCheck;

namespace ConsoleApp.HealthCheckers
{
    public class DummyHealthChecker : IHealthChecker
    {
        public string Key => "Dummy";
        public bool IsCritical => true;
        public async Task<bool> CheckHealth()
        {
            return true;
        }
    }
}