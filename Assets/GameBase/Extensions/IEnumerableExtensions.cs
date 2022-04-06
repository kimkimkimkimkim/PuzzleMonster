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

        public static List<T> InsertAllBetween<T>(this List<T> source, T value)
        {
            var list = new List<T>();
            source.ForEach((v, index) =>
            {
                list.Add(v);

                // 最後の要素でなければ指定した要素を挿入
                if(index != source.Count - 1)
                {
                    list.Add(value);
                }
            });

            return list;
        }
    }
}
