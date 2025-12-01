using System;
using System.Threading;

namespace Lab7v20
{
    public static class RetryHelper
    {
        public static T ExecuteWithRetry<T>(Func<T> operation, int retryCount = 3, TimeSpan? initialDelay = null, Func<Exception, bool>? shouldRetry = null)
        {
            if (initialDelay == null || initialDelay == TimeSpan.Zero)
                initialDelay = TimeSpan.FromMilliseconds(500);

            if (shouldRetry == null)
                shouldRetry = _ => true;

            int attempt = 0;

            while (true)
            {
                try
                {
                    attempt++;
                    Console.WriteLine($"[RetryHelper] Attempt #{attempt}...");
                    var result = operation();
                    Console.WriteLine("[RetryHelper] Operation succeeded.");
                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RetryHelper] Failure on attempt #{attempt}: {ex.Message}");

                    if (attempt > retryCount || !shouldRetry(ex))
                        throw;

                    var delay = TimeSpan.FromMilliseconds(initialDelay.Value.TotalMilliseconds * Math.Pow(2, attempt - 1));
                    Console.WriteLine($"[RetryHelper] Waiting {delay.TotalMilliseconds} ms...");
                    Thread.Sleep(delay);
                }
            }
        }
    }
}
