using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace Cig.Etl.Shared.Utils
{
    public static class Retry
    {
        public static void Do(
         Action action,
         TimeSpan retryInterval,
         int retryCount = 3)
        {
            Do<object>(() =>
            {
                action();
                return null;
            }, retryInterval, retryCount);
        }


        public static void Do(
            Action action,
           IRetryDelayStrategy defaultRetryDelayStrategy)
        {
            Do<object>(() =>
            {
                action();
                return null;
            }, defaultRetryDelayStrategy);
        }


        public static T Do<T>(Func<T> action, TimeSpan retryInterval, int retryCount = 3)
        {
            IRetryDelayStrategy defaultRetryDelayStrategy = new DefaultRetryDelayStrategy(retryInterval, retryCount);

            return Do(action, defaultRetryDelayStrategy);
        }

        public static T Do<T>(Func<T> action, IRetryDelayStrategy retryStrategy)
        {
            var exceptions = new List<Exception>();

            for (var retry = 0; retry < retryStrategy.RetryCount; retry++)
            {
                try
                {
                    if (retry > 0)
                    {
                        var waitDelay = retryStrategy.GetRetryDelay();
                        Thread.Sleep(Convert.ToInt32(waitDelay));
                    }
                    return action();
                }
                catch (InvalidCredentialException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            throw new AggregateException(exceptions);
        }

        public static async Task<T> DoAsync<T>(
            Func<T> action,
            TimeSpan retryInterval,
            int retryCount = 3)
        {
            var exceptions = new List<Exception>();

            for (var retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    if (retry > 0)
                    {
                        var waitDelay = (int)(Math.Pow(2, retry) * 1000 + retryInterval.TotalMilliseconds);
                        await Task.Delay(waitDelay).ConfigureAwait(false);
                    }
                    return action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            throw new AggregateException(exceptions);
        }
    }
}
