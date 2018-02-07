using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sillycore.BackgroundProcessing
{
    public class BackgroundJobManager
    {
        private readonly ILogger<BackgroundJobManager> _logger;
        private readonly List<BackgroundJobTimer> _timers = new List<BackgroundJobTimer>();

        public BackgroundJobManager(ILogger<BackgroundJobManager> logger)
        {
            _logger = logger;
        }

        public Task Start()
        {
            _logger.LogDebug("BackgroundJobManager: Start requested.");

            if (_timers != null && _timers.Any())
            {
                _logger.LogDebug($"BackgroundJobManager: {_timers.Count} job found.");
                foreach (BackgroundJobTimer timer in _timers)
                {
                    _logger.LogDebug($"BackgroundJobManager: Creating timer for job:{timer.JobType.FullName}.");
                    timer.InitTimer();
                }
            }

            return Task.CompletedTask;
        }

        public Task Stop()
        {
            _logger.LogDebug("BackgroundJobManager: Stop requested.");

            if (_timers != null && _timers.Any())
            {
                foreach (BackgroundJobTimer timer in _timers)
                {
                    _logger.LogDebug($"BackgroundJobManager: Disposing timer for job:{timer.JobType.FullName}.");
                    timer.Dispose();
                }
            }

            return Task.CompletedTask;
        }

        public void Register<T>(int jobIntervalInMs) where T : IJob
        {
            _logger.LogDebug($"BackgroundJobManager: Registering timer for job:{typeof(T).FullName}.");
            _timers.Add(new BackgroundJobTimer(typeof(T), jobIntervalInMs));
        }
    }
}