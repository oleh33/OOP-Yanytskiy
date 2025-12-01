using System;
using System.Collections.Generic;
using System.Linq;

namespace lab5v20
{
    public class DataFrame
    {
        private readonly List<DataRow> _rows = new();

        public void AddRow(DataRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            _rows.Add(row);
        }

        public IEnumerable<DataRow> Where(Func<DataRow, bool> predicate) =>
            _rows.Where(predicate);

        public IEnumerable<TResult> Select<TResult>(Func<DataRow, TResult> selector) =>
            _rows.Select(selector);

        public IEnumerable<IGrouping<object, DataRow>> GroupBy(string columnName)
        {
            return _rows.GroupBy(row =>
            {
                if (!row.Values.ContainsKey(columnName))
                    throw new ColumnNotFoundException(columnName);

                return row[columnName];
            });
        }

        public IReadOnlyList<DataRow> Rows => _rows;
    }
}
