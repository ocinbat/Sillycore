using System;
using System.Collections.Generic;

namespace Sillycore.Web.HealthCheck
{
    internal class HealthCheckerContainer
    {
        internal List<HealthChecker> HealthCheckers;

        public HealthCheckerContainer()
        {
            HealthCheckers = new List<HealthChecker>();
        }

        public void AddHealthChecker(Type healthCheckerType)
        {
            HealthChecker checker = new HealthChecker
            {
                HealthCheckerType = healthCheckerType
            };

            HealthCheckers.Add(checker);
        }
    }
}