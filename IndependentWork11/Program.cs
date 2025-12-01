using System;
using System.Net.Http;
using System.Threading;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;

internal class Program
{
    private static readonly Random _random = new Random();

    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=== IndependentWork11: Polly / Retry cases ===\n");

        Scenario1_ApiRetry();
        Console.WriteLine("\n---------------------------------------------\n");

        Scenario2_DatabaseRetryWithCircuitBreaker();
        Console.WriteLine("\n---------------------------------------------\n");

        Scenario3_QueueWithTimeoutAndFallback();

        Console.WriteLine("\n=== End of IndependentWork11 demo ===");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    // ------------------------------------------------------------
    // SCENARIO 1 — External API + Retry
    // ------------------------------------------------------------

    /*
     * Проблема:
     *  Зовнішній HTTP API може тимчасово бути недоступним.
     *  Якщо одразу падати — користувач не отримає даних, хоча API може "ожити" через секунду.
     *
     * Політика Polly:
     *  Retry з експоненційною затримкою (WaitAndRetry).
     */

    private static int _apiAttempts = 0;

    private static string CallExternalApi()
    {
        _apiAttempts++;
        Console.WriteLine($"[API] Attempt {_apiAttempts}: calling external service...");

        if (_apiAttempts <= 2)
        {
            // Імітуємо тимчасову помилку
            throw new HttpRequestException("Simulated temporary API error.");
        }

        // Імітація успішної відповіді
        return "Data from external API";
    }

    private static void Scenario1_ApiRetry()
    {
        Console.WriteLine("--- Scenario 1: External API call with Retry ---");

        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .WaitAndRetry(
                retryCount: 3,
                sleepDurationProvider: attempt =>
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // 2, 4, 8
                    return delay;
                },
                onRetry: (ex, delay, attempt, ctx) =>
                {
                    Console.WriteLine(
                        $"[Retry] attempt {attempt}, wait {delay.TotalSeconds}s, reason: {ex.Message}");
                });

        try
        {
            string result = retryPolicy.Execute(CallExternalApi);
            Console.WriteLine($"[API] Final result: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] Operation failed after all retries: {ex.Message}");
        }
    }

    // ------------------------------------------------------------
    // SCENARIO 2 — DB Retry + CircuitBreaker
    // ------------------------------------------------------------

    /*
     * Проблема:
     *  База даних може періодично падати / не відповідати.
     *  Якщо постійно дубасити її запитами — тільки гірше.
     *
     * Політика Polly:
     *  - Retry: кілька повторних спроб при помилках.
     *  - CircuitBreaker: якщо помилок забагато підряд — "відкриваємо коло" і тимчасово
     *    перестаємо ходити в БД.
     */

    private static int _dbAttempts = 0;

    private static string QueryDatabase()
    {
        _dbAttempts++;
        Console.WriteLine($"[DB] Attempt {_dbAttempts}: execute query...");

        // Імітуємо випадкові проблеми з підключенням (70% помилок)
        if (_random.NextDouble() < 0.7)
        {
            throw new Exception("Simulated database connection error.");
        }

        return "Some data from database";
    }

    private static void Scenario2_DatabaseRetryWithCircuitBreaker()
    {
        Console.WriteLine("--- Scenario 2: Database access with Retry + CircuitBreaker ---");

        // Retry на випадок одиночних помилок
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetry(
                retryCount: 2,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(500),
                onRetry: (ex, delay, attempt, ctx) =>
                {
                    Console.WriteLine(
                        $"[Retry-DB] attempt {attempt}, wait {delay.TotalMilliseconds}ms, reason: {ex.Message}");
                });

        // CircuitBreaker: після 3 помилок підряд відкриваємо коло на 5 секунд
        var breakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreaker(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(5),
                onBreak: (ex, breakDelay) =>
                {
                    Console.WriteLine($"[CircuitBreaker] OPEN for {breakDelay.TotalSeconds}s. Reason: {ex.Message}");
                },
                onReset: () =>
                {
                    Console.WriteLine("[CircuitBreaker] RESET (closed again).");
                },
                onHalfOpen: () =>
                {
                    Console.WriteLine("[CircuitBreaker] HALF-OPEN (test call).");
                });

        // Комбінуємо: спочатку breaker, всередині нього retry
        var combinedPolicy = Policy.Wrap(retryPolicy, breakerPolicy);

        for (int i = 1; i <= 8; i++)
        {
            Console.WriteLine($"\n[DB] Logical call #{i}");
            try
            {
                string data = combinedPolicy.Execute(QueryDatabase);
                Console.WriteLine($"[DB] Success: {data}");
            }
            catch (BrokenCircuitException)
            {
                Console.WriteLine("[DB] Circuit is OPEN — skipping call to protect DB.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB] Final failure: {ex.Message}");
            }

            Thread.Sleep(700); // невелика пауза між логічними викликами
        }
    }

    // ------------------------------------------------------------
    // SCENARIO 3 — Queue Timeout + Fallback
    // ------------------------------------------------------------

    /*
     * Проблема:
     *  Відправка повідомлення в чергу може "зависати" надовго,
     *  блокуючи потік.
     *
     * Політика Polly:
     *  - Timeout: обмежуємо максимальний час виконання.
     *  - Fallback: якщо таймаут — лог й "збереження локально" замість черги.
     */

    private static string SendToQueue(string message)
    {
        Console.WriteLine("[Queue] Sending message to remote queue...");

        // Випадкова затримка 0.5–3 секунди
        double delaySec = 0.5 + _random.NextDouble() * 2.5;
        Console.WriteLine($"[Queue] Simulated delay: {delaySec:F2} seconds");
        Thread.Sleep(TimeSpan.FromSeconds(delaySec));

        Console.WriteLine("[Queue] Message successfully sent to queue.");
        return "Message sent to queue.";
    }

    private static void Scenario3_QueueWithTimeoutAndFallback()
    {
        Console.WriteLine("--- Scenario 3: Queue send with Timeout + Fallback ---");

        // Timeout 2 секунди
        var timeoutPolicy = Policy.Timeout(
            timeout: TimeSpan.FromSeconds(2),
            timeoutStrategy: TimeoutStrategy.Pessimistic,
            onTimeout: (context, timeSpan, task) =>
            {
                Console.WriteLine($"[Timeout] Operation timed out after {timeSpan.TotalSeconds}s.");
            });

        // Fallback, якщо таймаут або інша помилка
        var fallbackPolicy = Policy<string>
            .Handle<TimeoutRejectedException>()
            .Or<Exception>()
            .Fallback(
                fallbackValue: "Message saved locally instead of queue.",
                onFallback: (res, ctx) =>
                {
                    Console.WriteLine("[Fallback] Queue send failed. Saving message locally.");
                });

        var combinedPolicy = fallbackPolicy.Wrap(timeoutPolicy);

        string result = combinedPolicy.Execute(
            () => SendToQueue("Important message"));

        Console.WriteLine($"[Queue] Final result: {result}");
    }
}
