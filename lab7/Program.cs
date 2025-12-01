using System;
using System.IO;
using System.Net.Http;

namespace Lab7v20
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Lab 7: IO/Network errors & Retry pattern ===\n");

            var fileProcessor = new FileProcessor();
            var networkClient = new NetworkClient();

            Func<Exception, bool> shouldRetry = ex =>
                ex is IOException || ex is HttpRequestException;

            try
            {
                Console.WriteLine(">>> Getting payload with Retry...\n");

                string payload = RetryHelper.ExecuteWithRetry(
                    () => fileProcessor.GetNotificationPayload("notification.json"),
                    retryCount: 5,
                    initialDelay: TimeSpan.FromMilliseconds(500),
                    shouldRetry: shouldRetry
                );

                Console.WriteLine($"\n[Main] Final payload: {payload}\n");

                Console.WriteLine(">>> Sending push notification with Retry...\n");

                bool sendResult = RetryHelper.ExecuteWithRetry(
                    () =>
                    {
                        networkClient.SendPushNotification("device-123", payload);
                        return true;
                    },
                    retryCount: 5,
                    initialDelay: TimeSpan.FromMilliseconds(500),
                    shouldRetry: shouldRetry
                );

                Console.WriteLine($"\n[Main] Send result: {sendResult}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Main] Operation failed: {ex.Message}");
            }

            Console.WriteLine("\n=== End of Lab 7 demo ===");
        }
    }
}
