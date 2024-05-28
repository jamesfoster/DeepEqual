namespace DeepEqual.Benchmark
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Syntax;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var object1 = new
            {
                Int = 1L,
                Enum = System.UriKind.Absolute,
                List = new List<int> { 1, 2, 3 },
                Inner = new
                {
                    X = 1,
                    Y = 2,
                    Z = 3
                },
                Set = new[] { 3, 4, 2, 1 },
                Dictionary = new Dictionary<int, int>
                {
                    { 2, 3 },
                    { 123, 234 },
                    { 345, 456 }
                },
                Custom = new Custom { Value = 10 }
            };

            var object2 = new
            {
                Int = 1,
                Enum = "Absolute",
                List = new[] { 1, 2, 3 },
                Inner = new Point
                {
                    X = 1,
                    Y = 2,
                    Z = 3
                },
                Set = new HashSet<int> { 1, 2, 3, 4 },
                Dictionary = new Dictionary<int, int>
                {
                    { 123, 234 },
                    { 345, 456 },
                    { 2, 3 }
                },
                Custom = new Custom { Value = 101 }
            };

            var sw = new Stopwatch();

            for (int i = 0; i < 10; i++)
            {
                sw.Restart();

                for (int j = 0; j < 100; j++)
                {
                    object1
                        .WithDeepEqual(object2)
                        .WithCustomComparison(new CustomComparison())
                        .Assert();
                }

                sw.Stop();

                Console.WriteLine("{0}ms", sw.ElapsedMilliseconds);
            }
        }
    }

    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
    }

    public class Custom
    {
        public int Value { get; set; }
    }

    public class CustomComparison : IComparison
    {
        public bool CanCompare(Type type1, Type type2)
        {
            return type1 == typeof(Custom) && type1 == type2;
        }

        public (ComparisonResult result, IComparisonContext context) Compare(
            IComparisonContext context,
            object value1,
            object value2
        )
        {
            var custom1 = (Custom)value1;
            var custom2 = (Custom)value2;

            var str1 = custom1.Value.ToString();
            var str2 = custom2.Value.ToString();

            if (str1.StartsWith(str2) || str2.StartsWith(str1))
            {
                return (ComparisonResult.Pass, context);
            }

            return (ComparisonResult.Fail, context.AddDifference(value1, value2));
        }
    }
}
