using System;
using System.Collections.Generic;

namespace lab5v20
{
    public class DataRow
    {
        private readonly Dictionary<string, object> _values = new();

        public void Add(string columnName, object value)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentException("Column name cannot be null or empty.", nameof(columnName));

            _values[columnName] = value;
        }

        public object this[string columnName]
        {
            get
            {
                if (!_values.ContainsKey(columnName))
                    throw new ColumnNotFoundException(columnName);

                return _values[columnName];
            }
        }

        public IReadOnlyDictionary<string, object> Values => _values;
    }
}
