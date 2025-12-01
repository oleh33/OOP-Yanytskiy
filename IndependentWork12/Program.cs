using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IndependentWork12
{
    /// <summary>
    /// Самостійна робота №12
    /// Тема: PLINQ: дослідження продуктивності та безпеки.
    ///
    /// У файлі Program.cs одночасно розміщено:
    ///  - код експериментів з LINQ vs PLINQ;
    ///  - приклад побічних ефектів у PLINQ;
    ///  - короткий "звіт" у вигляді коментарів.
    /// </summary>
    internal class Program
    {
        private static readonly Random _random = new Random();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("=== IndependentWork12: PLINQ performance & safety ===\n");

            // 1. Експерименти з продуктивності для різних розмірів колекції
            int[] sizes = { 1_000_000, 5_000_000, 10_000_000 };
            foreach (var size in sizes)
            {
                RunPerformanceExperiment(size);
                Console.WriteLine(new string('-', 60));
            }

            // 2. Демонстрація проблеми побічних ефектів у PLINQ
            Console.WriteLine();
            Console.WriteLine("=== Side-effects & thread-safety demo ===");
            DemonstrateSideEffectsProblem();
            Console.WriteLine();
            DemonstrateSideEffectsFixed();

            Console.WriteLine("\n=== Demo finished ===");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        #region Performance experiments

        /*
         * ОПИС ЕКСПЕРИМЕНТУ (для звіту)
         * --------------------------------
         * Колекції:       1M, 5M, 10M випадкових int у діапазоні [1; 1_000_000].
         * Операція:
         *   - фільтрація: залишаємо лише числа, які ймовірно є простими;
         *   - проекція: для кожного обчислюємо Math.Sqrt(n).
         *
         * Реалізація:
         *   1) Звичайний LINQ:        data.Where(IsPrime).Select(Math.Sqrt).ToList()
         *   2) PLINQ:                  data.AsParallel().Where(IsPrime).Select(Math.Sqrt).ToList()
         *
         * Вимірювання:
         *   - клас Stopwatch (System.Diagnostics);
         *   - для кожного розміру колекції виводиться час виконання LINQ та PLINQ.
         *
         * Коментар:
         *   - очікується, що для великих колекцій (5M, 10M) PLINQ буде швидше,
         *     тому що операція IsPrime досить "важка";
         *   - для менших колекцій або "легких" операцій накладні витрати паралелізації
         *     можуть перекрити виграш у швидкості.
         */

        private static void RunPerformanceExperiment(int size)
        {
            Console.WriteLine($"--- Performance experiment, size = {size:N0} elements ---");

            // 1. Створюємо вихідні дані
            var data = GenerateRandomData(size);

            // 2. Звичайний LINQ
            var sw = Stopwatch.StartNew();
            var linqResult = data
                .Where(IsPrime)
                .Select(n => Math.Sqrt(n))
                .ToList();
            sw.Stop();
            var linqTime = sw.Elapsed;

            Console.WriteLine($"LINQ : {linqTime.TotalMilliseconds:N0} ms, result count = {linqResult.Count}");

            // 3. PLINQ
            sw.Restart();
            var plinqResult = data
                .AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .Where(IsPrime)
                .Select(n => Math.Sqrt(n))
                .ToList();
            sw.Stop();
            var plinqTime = sw.Elapsed;

            Console.WriteLine($"PLINQ: {plinqTime.TotalMilliseconds:N0} ms, result count = {plinqResult.Count}");

            // 4. Короткий аналіз для поточного розміру
            if (plinqTime < linqTime)
            {
                Console.WriteLine("Коментар: для цього розміру колекції PLINQ виявився швидшим за LINQ.");
            }
            else
            {
                Console.WriteLine("Коментар: для цього розміру колекції PLINQ не дав виграшу (ймовірно, через накладні витрати паралелізації).");
            }
        }

        private static List<int> GenerateRandomData(int size)
        {
            var list = new List<int>(size);
            for (int i = 0; i < size; i++)
            {
                // випадкові числа у діапазоні [1; 1_000_000]
                list.Add(_random.Next(1, 1_000_001));
            }
            return list;
        }

        /// <summary>
        /// Досить "важка" перевірка на простоту числа.
        /// Не є оптимальним алгоритмом, але спеціально зроблений складним,
        /// щоб PLINQ мав шанс показати виграш.
        /// </summary>
        private static bool IsPrime(int n)
        {
            if (n <= 1) return false;
            if (n == 2) return true;
            if (n % 2 == 0) return false;

            int boundary = (int)Math.Sqrt(n);
            for (int i = 3; i <= boundary; i += 2)
            {
                if (n % i == 0) return false;
            }
            return true;
        }

        #endregion

        #region Side-effects & safety

        /*
         * СЦЕНАРІЙ ПОБІЧНИХ ЕФЕКТІВ (для звіту)
         * -------------------------------------
         * Проблема:
         *   - У PLINQ зручно використовувати ForAll / Select / Where та змінювати
         *     якісь зовнішні змінні всередині лямбда-виразів;
         *   - але при цьому кілька потоків одночасно змінюють спільний стан,
         *     що порушує потокобезпечність і призводить до некоректних результатів.
         *
         * Демонстрація:
         *   1) Є колекція з N елементів;
         *   2) Паралельно для кожного елемента робимо sum++ (звичайна змінна int);
         *   3) У результаті sum < N, тобто частина інкрементів "загубилась".
         *
         * Виправлення:
         *   - Використати lock;
         *   - або Interlocked.Increment;
         *   - або потікобезпечні колекції / агрегатор (наприклад, PLINQ.Aggregate).
         */

        private static void DemonstrateSideEffectsProblem()
        {
            Console.WriteLine("--- Problem: side-effects in PLINQ (no synchronization) ---");

            int count = 1_000_000;
            var data = Enumerable.Range(1, count).ToList();

            int sum = 0;

            // НЕКОРЕКТНИЙ код: кілька потоків одночасно змінюють sum
            data
                .AsParallel()
                .ForAll(x =>
                {
                    // побічний ефект: зміна зовнішньої змінної
                    sum++;
                });

            Console.WriteLine($"Очікуване значення sum = {count:N0}");
            Console.WriteLine($"Фактичне значення sum = {sum:N0} (як правило, менше через гонки потоків)");
        }

        private static void DemonstrateSideEffectsFixed()
        {
            Console.WriteLine("--- Fixed: side-effects with synchronization (Interlocked) ---");

            int count = 1_000_000;
            var data = Enumerable.Range(1, count).ToList();

            int sum = 0;

            // КОРЕКТНИЙ варіант: атомарний інкремент
            data
                .AsParallel()
                .ForAll(x =>
                {
                    Interlocked.Increment(ref sum);
                });

            Console.WriteLine($"Очікуване значення sum = {count:N0}");
            Console.WriteLine($"Фактичне значення sum = {sum:N0} (збігається завдяки потокобезпечній операції)");
        }

        #endregion
    }
}
