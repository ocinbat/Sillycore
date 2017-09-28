using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sillycore.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsEmpty<T>(this T[] source)
        {
            return source == null || !source.Any();
        }

        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }

        public static bool HasElements<T>(this T[] source)
        {
            return !IsEmpty(source);
        }

        public static bool HasElements<T>(this IEnumerable<T> source)
        {
            return !IsEmpty(source);
        }

        /// <summary>
        /// Taken from https://github.com/elastic/elasticsearch-net-example/blob/master/src/NuSearch.Domain/Extensions/PartitionExtension.cs
        /// </summary>
        /// <typeparam name="T">Generic IEnumerable type</typeparam>
        /// <param name="source">source</param>
        /// <param name="size">each partition size</param>
        /// <returns>a list of partitions</returns>
        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> source, int size)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "source cannot be null.");
            }

            T[] array = null;
            int count = 0;

            foreach (T item in source)
            {
                if (array == null)
                {
                    array = new T[size];
                }

                array[count] = item;

                count++;

                if (count == size)
                {
                    yield return new ReadOnlyCollection<T>(array);
                    array = null;
                    count = 0;
                }
            }

            if (array != null)
            {
                Array.Resize(ref array, count);
                yield return new ReadOnlyCollection<T>(array);
            }
        }
    }

}