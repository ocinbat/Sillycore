using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sillycore.BackgroundProcessing
{
    public class BackgroundJobManager
    {
        public bool IsActive { get; set; }
        public List<BackgroundJobTimer> JobTimers { get; set; }

        private readonly ILogger<BackgroundJobManager> _logger;

        public BackgroundJobManager(ILogger<BackgroundJobManager> logger)
        {
            _logger = logger;
            JobTimers = new List<BackgroundJobTimer>();
        }

        public Task Start()
        {
            _logger.LogDebug("BackgroundJobManager: Start requested.");

            if (JobTimers.Any())
            {
                _logger.LogDebug($"BackgroundJobManager: {JobTimers.Count} job found.");
                foreach (BackgroundJobTimer timer in JobTimers)
                {
                    _logger.LogDebug($"BackgroundJobManager: Creating timer for job:{timer.JobType.FullName}.");
                    timer.InitTimer();
                }
            }

            IsActive = true;
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            _logger.LogDebug("BackgroundJobManager: Stop requested.");

            if (JobTimers.Any())
            {
                foreach (BackgroundJobTimer timer in JobTimers)
                {
                    _logger.LogDebug($"BackgroundJobManager: Disposing timer for job:{timer.JobType.FullName}.");
                    timer.Dispose();
                }
            }

            IsActive = false;
            return Task.CompletedTask;
        }

        public void Register<T>(int jobIntervalInMs) where T : IJob
        {
            _logger.LogDebug($"BackgroundJobManager: Registering timer for job:{typeof(T).FullName}.");
            JobTimers.Add(new BackgroundJobTimer(typeof(T), jobIntervalInMs));
        }
    }
}