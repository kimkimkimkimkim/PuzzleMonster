using System;
using System.Collections.Generic;
using System.Linq;

namespace GameBase
{
    public static class IEnumerableExtensions
    {
        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource, int> predicate)
        {
            var index = 0;
            foreach (TSource element in source)
            {
                predicate(element, index++);
            }
        }

        public static IEnumerable<TSource> Shuffle<TSource>(this IEnumerable<TSource> source)
        {
            return source.OrderBy(_ => Guid.NewGuid());
        }
    }
}
