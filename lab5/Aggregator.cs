using System;
using System.Collections.Generic;
using System.Linq;

namespace lab5v20
{
    public static class Aggregator<T>
        where T : struct, IConvertible
    {
        public static double Sum(IEnumerable<T> source)
            => source.Select(x => Convert.ToDouble(x)).Sum();

        public static double Avg(IEnumerable<T> source)
        {
            var list = source.ToList();
            return list.Count == 0 ? 0 : list.Select(x => Convert.ToDouble(x)).Average();
        }

        public static int Count(IEnumerable<T> source)
            => source.Count();
    }
}
