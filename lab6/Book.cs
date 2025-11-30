using System;

namespace OOP_Yanytskiy.Lab6
{
    public class Book
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public int Year { get; set; }
        public double Rating { get; set; }

        public Book(string title, string author, int year, double rating)
        {
            Title = title;
            Author = author;
            Year = year;
            Rating = rating;
        }

        public override string ToString()
        {
            return $"{Title} | {Author} | {Year} | Rating: {Rating}";
        }
    }
}
