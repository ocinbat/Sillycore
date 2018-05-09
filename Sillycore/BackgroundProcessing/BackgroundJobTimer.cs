using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sillycore.Extensions;

namespace Sillycore.BackgroundProcessing
{
    public class BackgroundJobTimer : IDisposable
    {
        public string Name { get; set; }
        public BackgroundJobStatus Status { get; set; }
        public long FailuresInARow { get; set; }
        public string LastError { get; set; }
        public DateTime? ExecutedOn { get; set; }

        private static readonly ILogger<BackgroundJobTimer> Logger = SillycoreApp.Instance.LoggerFactory.CreateLogger<BackgroundJobTimer>();
        private static readonly IConfiguration Configuration = SillycoreApp.Instance.Configuration;

        private readonly string _configurationKeyForIntervalInMs;
        private readonly int _failureThreshold;
        internal readonly Type JobType;

        private Timer _timer;
        private bool _isRunning;
        private int _intervalInMs;

        public BackgroundJobTimer(Type jobType, string configurationKeyForIntervalInMs, int? failureThreshold = 3) :
            this(jobType, Configuration[configurationKeyForIntervalInMs].ToInt(), failureThreshold)
        {
            _configurationKeyForIntervalInMs = configurationKeyForIntervalInMs;
        }

        public BackgroundJobTimer(Type jobType, int intervalInMs, int? failureThreshold = 3)
        {
            JobType = jobType;
            _intervalInMs = intervalInMs;
            _failureThreshold = failureThreshold ?? 3;
            Name = jobType.Name;
            Status = BackgroundJobStatus.Idle;
            FailuresInARow = 0;
        }

        public void InitTimer()
        {
            Logger.LogDebug($"BackgroundJobManager: Initializing timer for job:{JobType.FullName} with interval:{_intervalInMs}ms.");
            if (_timer == null)
            {
                _timer = new Timer(TimerHandler, null, _intervalInMs, _intervalInMs);
                Logger.LogDebug($"BackgroundJobManager: Initialized timer for job:{JobType.FullName}.");
            }
        }

        public void Dispose()
        {
            Logger.LogDebug($"BackgroundJobManager: Disposing timer for job:{JobType.FullName}.");
            if (_timer != null)
            {
                lock (this)
                {
                    _timer.Dispose();
                    _timer = null;

                    while (_isRunning)
                    {
                        Logger.LogDebug($"BackgroundJobManager: Waiting current operation for job:{JobType.FullName} to be finished before disposing.");
                        Thread.Sleep(100);
                    }

                    Logger.LogDebug($"BackgroundJobManager: Job:{JobType.FullName} finished and disposed.");
                }
            }
        }

        #region Helpers

        private void TimerHandler(object state)
        {
            _timer.Change(-1, -1);

            Run().Wait();

            if (_timer != null)
            {
                if (!String.IsNullOrWhiteSpace(_configurationKeyForIntervalInMs))
                {
                    _intervalInMs = Configuration[_configurationKeyForIntervalInMs].ToInt();
                }

                _timer.Change(_intervalInMs, _intervalInMs);
            }
        }

        private async Task Run()
        {
            Logger.LogDebug($"BackgroundJobManager: Timer hit for job:{JobType.FullName}.");
            if (_isRunning)
            {
                Logger.LogDebug($"BackgroundJobManager: Timer hit for job:{JobType.FullName} but previous instance is still running. Iteration is cancelled and timer is reset.");
                return;
            }

            _isRunning = true;
            Status = BackgroundJobStatus.Running;

            try
            {
                Logger.LogDebug($"BackgroundJobManager: Trying to execute job:{JobType.FullName}.");
                await RunTaskInNewThread();
                Logger.LogDebug($"BackgroundJobManager: Execution of job:{JobType.FullName} finished.");
            }
            catch (Exception ex)
            {
                FailuresInARow++;
                Logger.LogError(ex, $"BackgroundJobManager: Error occured while activating job:{JobType.FullName}.");
                LastError = ex.Message;
            }
            finally
            {
                ExecutedOn = SillycoreApp.Instance.DateTimeProvider.Now;
                Status = BackgroundJobStatus.Idle;
                _isRunning = false;

                if (FailuresInARow > _failureThreshold)
                {
                    Status = BackgroundJobStatus.Failing;
                }
            }
        }

        private async Task RunTaskInNewThread()
        {
            try
            {
                var scopeFactory = SillycoreApp.Instance.ServiceProvider.GetService<IServiceScopeFactory>();

                using (var scope = scopeFactory.CreateScope())
                {
                    Logger.LogDebug($"BackgroundJobManager: Trying to create a new instance of job:{JobType.FullName}.");
                    IJob job = (IJob)scope.ServiceProvider.GetService(JobType);

                    if (job != null)
                    {
                        Logger.LogDebug($"BackgroundJobManager: Running job:{JobType.FullName}.");
                        await job.Run();
                        FailuresInARow = 0;
                        LastError = null;
                        Logger.LogDebug($"BackgroundJobManager: Job:{JobType.FullName} run successfully.");
                    }
                    else
                    {
                        FailuresInARow++;
                        Logger.LogError($"BackgroundJobManager: Job:{JobType.FullName} cannot be initialized.");
                        LastError = $"Job:{JobType.FullName} cannot be initialized.";
                    }
                }
            }
            catch (Exception ex)
            {
                FailuresInARow++;
                Logger.LogError(ex, $"BackgroundJobManager: Error occured while running job:{JobType.FullName}.");
                LastError = ex.Message;
            }

            GC.Collect();
        }

        #endregion
    }
}