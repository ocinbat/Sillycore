using System;

namespace Sillycore.RabbitMq.Configuration
{
    public class IncrementalRetryPolicyConfiguration
    {
        public int RetryLimit { get; set; }
        public int InitialIntervalInMs { get; set; }
        public int IntervalIncrementInMs { get; set; }
        
        internal TimeSpan InitialInterval => TimeSpan.FromMilliseconds(InitialIntervalInMs);
        internal TimeSpan IntervalIncrement => TimeSpan.FromMilliseconds(IntervalIncrementInMs);
    }
}