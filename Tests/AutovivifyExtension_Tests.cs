using Xunit;
using StaffGenerator;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Tests
{
    public class AutovivifyExtension_Tests
    {
        [Fact]
        public void Test1()
        {
            var dict = new Dictionary<int, List<int>>();

            var key1 = 42;
            var key2 = 3;

            Assert.DoesNotContain(key1, dict.Keys);
            Assert.DoesNotContain(key2, dict.Keys);

            var expected1 = dict.Autovivify(key1);
            var expected2 = dict.Autovivify(key2);

            Assert.True(dict.TryGetValue(key1, out var actual1));
            Assert.True(dict.TryGetValue(key2, out var actual2));

            Assert.Same(expected1, actual1);
            Assert.Same(expected2, actual2);
            Assert.NotSame(actual1, actual2);
        }

        [Fact]
        public void Test2()
        {
            var dict = new Dictionary<int, ImmutableHashSet<int>.Builder>();

            var key1 = 42;
            var key2 = 3;

            Assert.DoesNotContain(key1, dict.Keys);
            Assert.DoesNotContain(key2, dict.Keys);

            var expected1 = dict.Autovivify(key1);
            var expected2 = dict.Autovivify(key2);

            Assert.True(dict.TryGetValue(key1, out var actual1));
            Assert.True(dict.TryGetValue(key2, out var actual2));

            Assert.Same(expected1, actual1);
            Assert.Same(expected2, actual2);
            Assert.NotSame(actual1, actual2);
        }

        record Foo(List<int> a, Dictionary<int, int> b);

        [Fact]
        public void Test3()
        {
            var dict = new Dictionary<int, Foo>();

            var key1 = 42;
            var key2 = 3;

            Assert.DoesNotContain(key1, dict.Keys);
            Assert.DoesNotContain(key2, dict.Keys);

            var expected1 = dict.Autovivify(key1, () => new(new(), new()));
            var expected2 = dict.Autovivify(key2, () => new(new(), new()));

            Assert.True(dict.TryGetValue(key1, out var actual1));
            Assert.True(dict.TryGetValue(key2, out var actual2));
            Assert.NotNull(actual1?.a);
            Assert.NotNull(actual1?.b);
            Assert.NotNull(actual2?.a);
            Assert.NotNull(actual2?.b);

            Assert.Same(expected1, actual1);
            Assert.Same(expected2, actual2);
            Assert.NotSame(actual1, actual2);
            Assert.NotSame(actual1?.a, actual2?.a);
            Assert.NotSame(actual1?.b, actual2?.b);
        }
    }
}
