using System;
using System.Collections.Generic;
using System.Linq;

namespace Iridium.Reflection.Test
{
    public static class LINQExtensions
    {
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };

            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                    from accseq in accumulator
                    from item in sequence
                    select accseq.Concat(new[] { item }));
        }

        public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource item)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            yield return item;

            foreach (var element in source)
                yield return element;
        }

        public static IEnumerable<IEnumerable<TSource>> Combinate<TSource>(this IEnumerable<TSource> source, int k)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var list = source.ToList();
            if (k > list.Count)
                throw new ArgumentOutOfRangeException("k");

            if (k == 0)
                yield return Enumerable.Empty<TSource>();

            foreach (var l in list)
            foreach (var c in Combinate(list.Skip(list.Count - k - 2), k - 1))
                yield return c.Prepend(l);
        }

        public static IEnumerable<IEnumerable<T>> PowerSet<T>(this IEnumerable<T> items)
        {
            var list = items.ToList();
            
            return (from m in Enumerable.Range(0, 1 << list.Count)
                    select
                        from i in Enumerable.Range(0, list.Count)
                        where (m & (1 << i)) != 0
                        select list[i])
                .Skip(1);
        }

        public static IEnumerable<TypeFlags> AllCombinations(this IEnumerable<TypeFlags> list)
        {
            foreach (var combo in list.PowerSet())
            {
                yield return combo.Aggregate<TypeFlags, TypeFlags>(0, (current, flag) => current | flag);
            }
        }

        

    }
}