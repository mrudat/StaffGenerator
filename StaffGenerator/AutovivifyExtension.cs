using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace StaffGenerator
{
    public static class AutovivifyExtension
    {
        public static V Autovivify<K, V>(this IDictionary<K, V> dict, K key)
            where K : notnull where V : new()
        {
            return dict.Autovivify(key, () => new());
        }

        public static ImmutableHashSet<V>.Builder Autovivify<K, V>(this IDictionary<K, ImmutableHashSet<V>.Builder> dict, K key)
            where K : notnull
        {
            return dict.Autovivify(key, () => ImmutableHashSet.CreateBuilder<V>());
        }

        public static V Autovivify<K, V>(this IDictionary<K, V> dict, K key, Func<V> newThing) where K : notnull
        {
            if (!dict.TryGetValue(key, out var value))
                value = dict[key] = newThing();
            return value;
        }
    }
}
