using System;
using System.Collections.Generic;
using System.Linq;

namespace OOP_Yanytskiy.Lab6
{
    public static class BookLinqDemo
    {
        public static void Run()
        {
            List<Book> books = new List<Book>
            {
                new Book("Clean Code", "Robert C. Martin", 2008, 4.8),
                new Book("The Pragmatic Programmer", "Andrew Hunt", 1999, 4.7),
                new Book("Code Complete", "Steve McConnell", 2004, 4.6),
                new Book("Introduction to Algorithms", "Thomas H. Cormen", 2009, 4.3),
                new Book("Refactoring", "Martin Fowler", 1999, 4.5),
                new Book("Design Patterns", "Erich Gamma", 1994, 4.2),
                new Book("Clean Architecture", "Robert C. Martin", 2017, 4.4),
                new Book("The Clean Coder", "Robert C. Martin", 2011, 4.6)
            };

            Console.WriteLine("=== All books ===");
            foreach (var b in books)
                Console.WriteLine(b);

            Predicate<Book> highRatingPredicate = b => b.Rating > 4.5;

            var highRatedBooks = books.Where(b => highRatingPredicate(b)).ToList();

            Console.WriteLine("\n=== Books with rating > 4.5 ===");
            highRatedBooks.ForEach(b => Console.WriteLine(b));

            var booksByAuthor = books.GroupBy(b => b.Author).OrderBy(g => g.Key);

            Console.WriteLine("\n=== Books grouped by author ===");
            foreach (var group in booksByAuthor)
            {
                Console.WriteLine($"\nAuthor: {group.Key}");
                foreach (var book in group.OrderBy(b => b.Year))
                    Console.WriteLine($"  {book.Year} - {book.Title} (Rating: {book.Rating})");
            }

            var orderedByYear = books.OrderBy(b => b.Year).ToList();

            Console.WriteLine("\n=== Books ordered by year ===");
            orderedByYear.ForEach(b => Console.WriteLine(b));

            var averageRating = books
                .Select(b => b.Rating)
                .Aggregate(0.0, (acc, r) => acc + r) / books.Count;

            Console.WriteLine($"\nAverage rating: {averageRating:F2}");
        }
    }
}
