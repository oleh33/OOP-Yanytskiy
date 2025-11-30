using System;
using System.Threading;

namespace OOP_Yanytskiy.Lab7
{
    /// <summary>
    /// Допоміжний клас для виконання операцій з патерном Retry.
    /// </summary>
    public static class RetryHelper
    {
        /// <summary>
        /// Виконує операцію з повторними спробами і експоненційною затримкою.
        /// </summary>
        public static T ExecuteWithRetry<T>(
            Func<T> operation,
            int retryCount = 3,
            TimeSpan initialDelay = default,
            Func<Exception, bool> shouldRetry = null)
        {
            if (initialDelay == default)
                initialDelay = TimeSpan.FromMilliseconds(500);

            if (shouldRetry == null)
                shouldRetry = _ => true;

            int attempt = 0;
            Exception lastException = null;

            while (attempt <= retryCount)
            {
                try
                {
                    attempt++;
                    Console.WriteLine($"[RetryHelper] Attempt #{attempt}...");

                    T result = operation();
                    Console.WriteLine("[RetryHelper] Success.");
                    return result;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    Console.WriteLine($"[RetryHelper] Failure on attempt #{attempt}: {ex.GetType().Name} - {ex.Message}");

                    if (attempt > retryCount || !shouldRetry(ex))
                    {
                        Console.WriteLine("[RetryHelper] No more retries. Throwing exception.");
                        throw;
                    }

                    var delay = TimeSpan.FromMilliseconds(initialDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
                    Console.WriteLine($"[RetryHelper] Waiting {delay.TotalMilliseconds} ms before next attempt...");
                    Thread.Sleep(delay);
                }
            }

            throw lastException ?? new Exception("Unknown error in RetryHelper.");
        }
    }
}
