namespace Sillycore.Web.HealthCheck
{
    public interface IHealthChecker
    {
        string Key { get; }

        bool IsCritical { get; }

        bool CheckHealth();
    }
}