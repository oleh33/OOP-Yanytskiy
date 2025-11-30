using System;

namespace OOP_Yanytskiy.Lab6
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            DelegatesDemo.Run();

            Console.WriteLine("\n----------------------------------------\n");

            BookLinqDemo.Run();

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
