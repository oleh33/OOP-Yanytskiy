using System;
using System.Linq;

namespace lab5v20
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("--- Lab 5 v20: DataFrame / Generics / LINQ / Exceptions ---\n");

            var df = new DataFrame();

            // Створюємо тестові дані
            for (int i = 0; i < 10; i++)
            {
                var row = new DataRow();
                row.Add("Name", $"Product {i}");
                row.Add("Price", 10 + i * 3);
                row.Add("Quantity", i + 1);
                row.Add("Category", (i % 2 == 0) ? "Food" : "Tech");
                df.AddRow(row);
            }

            Console.WriteLine("DataFrame created with 10 rows.");

            // LINQ-фільтрація
            Console.WriteLine("\nProducts with price > 20:");
            var expensive = df.Where(r => Convert.ToDouble(r["Price"]) > 20);
            foreach (var r in expensive)
            {
                Console.WriteLine($"{r["Name"]}: {r["Price"]} ({r["Category"]})");
            }

            // Агрегування: сума та середнє
            var prices = df.Select(r => Convert.ToDouble(r["Price"]));
            Console.WriteLine($"\nTotal price (Sum): {Aggregator<double>.Sum(prices):0.00}");
            Console.WriteLine($"Average price (Avg): {Aggregator<double>.Avg(prices):0.00}");

            // GroupBy по колонці Category
            Console.WriteLine("\nGrouped by Category:");
            var grouped = df.GroupBy("Category");
            foreach (var group in grouped)
            {
                Console.WriteLine($"Category '{group.Key}' -> {group.Count()} items");
            }

            // Демонстрація винятку
            try
            {
                Console.WriteLine("\nTrying to access non-existing column...");
                var x = df.Rows.First()["NotExists"];
            }
            catch (ColumnNotFoundException ex)
            {
                Console.WriteLine("ERROR → " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error: " + ex.Message);
            }

            Console.WriteLine("\n--- End of Lab 5 v20 ---");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
