using System;
using System.Collections.Concurrent;

namespace StaffGenerator
{
    public static class MemoizeExtension
    {
        public static Func<T1, TResult> Memoize<T1, TResult>(this Func<T1, TResult> func)
            where T1 : notnull
        {
            var cache = new ConcurrentDictionary<T1, TResult>();
            return a => cache.GetOrAdd(a, func);
        }

        public static Func<T1, T2, TResult> Memoize<T1, T2, TResult>(this Func<T1, T2, TResult> func)
        {
            var cache = new ConcurrentDictionary<(T1 a, T2 b), TResult>();
            return (a, b) => cache.GetOrAdd((a, b), (x) => func(x.a, x.b));
        }

        public static Func<T1, T2, T3, TResult> Memoize<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> func)
        {
            var cache = new ConcurrentDictionary<(T1 a, T2 b, T3 c), TResult>();
            return (a, b, c) => cache.GetOrAdd((a, b, c), (x) => func(x.a, x.b, x.c));
        }
    }
}
