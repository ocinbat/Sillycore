using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Sillycore.Extensions;

namespace Sillycore.Logging
{
    public class LogContext : IDisposable
    {
        private readonly ILogger _logger;
        private string _message;
        private int _thresholdInMs = 0;
        private bool _logExecutionTime = false;
        private readonly bool _logExecutionTimeGlobal = SillycoreApp.Instance?.Configuration["LogExecutionTime"].ToBool() ?? false;

        private readonly Stopwatch _sw;

        public LogContext(ILogger logger)
        {
            _logger = logger;
            _sw = Stopwatch.StartNew();
        }

        public LogContext WithMessage(string message)
        {
            _message = message;
            return this;
        }

        public LogContext WithThresholdInMs(int thresholdIsMs)
        {
            _thresholdInMs = thresholdIsMs;
            return this;
        }

        public LogContext WithExecutionTime()
        {
            _logExecutionTime = true;
            return this;
        }

        public void Dispose()
        {
            if (_logExecutionTime || _logExecutionTimeGlobal)
            {
                _sw.Stop();
                long elapsedMs = _sw.ElapsedMilliseconds;

                if (_thresholdInMs < 1 || elapsedMs > _thresholdInMs)
                {
                    _logger.LogInformation($"{_message} took {elapsedMs} miliseconds");
                }
            }
            else
            {
                _logger.LogInformation($"{_message}");
            }
        }
    }
}