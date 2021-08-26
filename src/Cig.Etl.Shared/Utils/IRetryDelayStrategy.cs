using System;

namespace Cig.Etl.Shared.Utils
{
    public interface IRetryDelayStrategy
    {
        TimeSpan RetryInterval { get; }
        int RetryCount { get; }
        double GetRetryDelay();
    }
}