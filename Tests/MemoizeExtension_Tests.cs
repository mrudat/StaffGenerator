using System;
using Xunit;
using StaffGenerator;

namespace Tests
{

    public class MemoizeExtension_Tests
    {
        [Theory]
        [InlineData(1, 0)]
        [InlineData(2, 1)]
        public void Test1(int expected, int v1)
        {
            Func<int,int> f = (x) => x + 1;

            f = f.Memoize();

            Assert.Equal(expected, f(v1));
            Assert.Equal(expected, f(v1));
        }

        [Theory]
        [InlineData(1, 1, 0)]
        [InlineData(2, 1, 1)]
        public void Test2(int expected, int v1, int v2)
        {
            Func<int, int, int> f = (x, y) => x << y;

            f = f.Memoize();

            Assert.Equal(expected, f(v1, v2));
        }

        [Theory]
        [InlineData(2, 1, 0, 1)]
        [InlineData(3, 1, 1, 1)]
        public void Test3(int expected, int v1, int v2, int v3)
        {
            Func<int, int, int, int> f = (x, y, z) => (x << y) + z;

            f = f.Memoize();

            Assert.Equal(expected, f(v1, v2, v3));
        }
    }
}
