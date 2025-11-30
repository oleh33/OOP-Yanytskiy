using System;
using System.Collections.Generic;
using System.Linq;

namespace OOP_Yanytskiy.Lab6
{
    public delegate double BinaryOperation(double x, double y);

    public static class DelegatesDemo
    {
        public static void Run()
        {
            Console.WriteLine("=== Delegates & Lambda Demo ===");

            BinaryOperation add = delegate (double a, double b)
            {
                return a + b;
            };

            BinaryOperation multiply = (x, y) => x * y;

            Console.WriteLine($"Add (anon method): 5 + 7 = {add(5, 7)}");
            Console.WriteLine($"Multiply (lambda): 5 * 7 = {multiply(5, 7)}");

            Func<int, int> square = x => x * x;
            Func<int, int, int> max = (x, y) => x > y ? x : y;

            Console.WriteLine($"\nSquare (Func): 6^2 = {square(6)}");
            Console.WriteLine($"Max (Func): max(10, 3) = {max(10, 3)}");

            Action<string> printLine = s => Console.WriteLine($"[Action] {s}");
            printLine("Hello from Action delegate!");

            Predicate<int> isEven = n => n % 2 == 0;

            Console.WriteLine($"\nIsEven(10): {isEven(10)}");
            Console.WriteLine($"IsEven(7): {isEven(7)}");

            List<int> numbers = new List<int> { 1,2,3,4,5,6,7,8,9,10 };

            var evenNumbers = numbers.Where(n => isEven(n)).ToList();
            var oddSquares = numbers.Where(n => !isEven(n)).Select(n => square(n));

            Console.WriteLine("\nEven numbers: " + string.Join(", ", evenNumbers));
            Console.WriteLine("Odd squares: " + string.Join(", ", oddSquares));

            int sum = numbers.Aggregate(0, (acc, n) => acc + n);
            Console.WriteLine($"\nSum (Aggregate): {sum}");
        }
    }
}
