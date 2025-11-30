using System;
using System.IO;
using System.Net.Http;

namespace OOP_Yanytskiy.Lab7
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== Lab 7: IO/Network errors & Retry pattern ===\n");

            var fileProcessor = new FileProcessor();
            var networkClient = new NetworkClient();

            Func<Exception, bool> shouldRetryForIoAndHttp = ex =>
                ex is IOException || ex is HttpRequestException;

            try
            {
                Console.WriteLine(">>> Getting notification payload with Retry...\n");

                string payload = RetryHelper.ExecuteWithRetry(
                    operation: () => fileProcessor.GetNotificationPayload("notification.json"),
                    retryCount: 5,
                    initialDelay: TimeSpan.FromMilliseconds(500),
                    shouldRetry: shouldRetryForIoAndHttp
                );

                Console.WriteLine($"\n[Main] Final payload: {payload}\n");

                Console.WriteLine(">>> Sending push notification with Retry...\n");

                bool sendResult = RetryHelper.ExecuteWithRetry(
                    operation: () =>
                    {
                        networkClient.SendPushNotification("device-123", payload);
                        return true;
                    },
                    retryCount: 5,
                    initialDelay: TimeSpan.FromMilliseconds(500),
                    shouldRetry: shouldRetryForIoAndHttp
                );

                Console.WriteLine($"\n[Main] Send result: {sendResult}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Main] Operation failed: {ex.GetType().Name} - {ex.Message}");
            }

            Console.WriteLine("\n=== End of Lab 7 demo ===");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
