using System;

namespace lab5v20
{
    public class ColumnNotFoundException : Exception
    {
        public ColumnNotFoundException(string column)
            : base($"Column '{column}' does not exist in DataFrame.")
        {
        }
    }
}
