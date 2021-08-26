using System;

namespace Cig.Etl.Shared.Utils
{
    public class DefaultRetryDelayStrategy : IRetryDelayStrategy
    {

        private readonly int RetryTimeSpanInSeconds = 10;
        public TimeSpan RetryInterval { get; }
        public int RetryCount { get; }

        public DefaultRetryDelayStrategy()
        {
            RetryInterval = TimeSpan.FromSeconds(RetryTimeSpanInSeconds);
            RetryCount = 3;
        }

        public DefaultRetryDelayStrategy(TimeSpan retryInterval, int retryCount)
        {
            RetryInterval = retryInterval;
            RetryCount = retryCount;
        }

        public double GetRetryDelay()
        {
            return Math.Pow(2, RetryCount) * 100000 + RetryInterval.TotalMilliseconds;
        }
    }
}
