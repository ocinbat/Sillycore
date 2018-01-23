using System;
using Microsoft.Extensions.Logging;
using Sillycore.Logging;

namespace Sillycore.Extensions
{
    public static class LoggerExtensions
    {
        public static LogContext LogExecutionTime(this ILogger logger, string message = "")
        {
            LogContext context = new LogContext(logger);

            if (!String.IsNullOrWhiteSpace(message))
            {
                context.WithMessage(message);
            }

            return context.WithExecutionTime();
        }
    }
}