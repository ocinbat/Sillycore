using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sillycore.BackgroundProcessing
{
    public class BackgroundJobTimer : IDisposable
    {
        private static readonly ILogger Logger = SillycoreApp.Instance.LoggerFactory.CreateLogger<ILogger>();
        private readonly int _intervalInMs;
        internal readonly Type JobType;

        private Timer _timer;
        private bool _isRunning;

        public BackgroundJobTimer(Type jobType, int intervalInMs)
        {
            JobType = jobType;
            _intervalInMs = intervalInMs;
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

            try
            {
                Logger.LogDebug($"BackgroundJobManager: Trying to execute job:{JobType.FullName}.");
                await RunTaskInNewThread();
                Logger.LogDebug($"BackgroundJobManager: Execution of job:{JobType.FullName} successfull.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"BackgroundJobManager: Error occured while activating job:{JobType.FullName}.");
            }
            finally
            {
                _isRunning = false;
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
                        Logger.LogDebug($"BackgroundJobManager: Job:{JobType.FullName} run.");
                    }
                    else
                    {
                        Logger.LogDebug($"BackgroundJobManager: Job:{JobType.FullName} cannot be initialized.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"BackgroundJobManager: Error occured while running job:{JobType.FullName}.");
            }

            GC.Collect();
        }

        #endregion
    }
}