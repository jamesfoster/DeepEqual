using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using ApprovalTests;
using ApprovalTests.Reporters;

using DeepEqual.Formatting;
using DeepEqual.Syntax;
using DeepEqual.Test.Helper;

using Xunit;

namespace DeepEqual.Test;

public class DeepComparisonTest
{
    [Fact]
    public void KitchenSink()
    {
        var object1 = new
            {
                A = 1,
                B = UriKind.Absolute,
                C = new List<int> {1, 2, 3},
                Float = 1.111_111_8f,
                Double = 1.111_111_111_111_118d,
                Inner = new
                    {
                        X = 1,
                        Y = 2,
                        Z = 3
                    },
                Set = new[] {3, 4, 2, 1},
                Dictionary = new Dictionary<int, int>
                    {
                        {2, 3},
                        {123, 234},
                        {345, 456}
                    },
                X = new Left(123)
            };

        var object2 = new
            {
                A = 1,
                B = "Absolute",
                C = new[] {1, 2, 3},
                Float = 1.111_111_9m,
                Double = 1.111_111_111_111_119m,
                Inner = new TestType
                    {
                        X = 1,
                        Y = 3,
                        Z = 3
                    },
                Set = new HashSet<int> {1, 2, 3, 4},
                Dictionary = new Dictionary<int, int>
                    {
                        {123, 234},
                        {345, 456},
                        {2, 3}
                    },
                X = new Right(123)
            };

        var comparison = new ComparisonBuilder()
            .IgnoreProperty<TestType>(x => x.Y)
            .MapProperty<Left, Right>(x => x.Size, x => x.Width)
            .Create();

        DeepAssert.AreEqual(object1, object2, comparison);
    }

    [Fact]
    [UseReporter(typeof(DiffReporter))]
    public void KitchenSinkFailures()
    {
        var object1 = new
        {
            A = 1,
            B = UriKind.Absolute,
            C = new List<int> { 1, 2, 3 },
            Float = 1.111_111_6f,
            Double = 1.111_111_111_111_116d,
            String = "a1b2c3",
            Inner = new
            {
                X = 1,
                Y = 2,
                Z = 3
            },
            Set = new[] { 3, 4, 2, 1 },
            Dictionary = new Dictionary<int, int>
            {
                {2, 3},
                {123, 234},
                {345, 456}
            },
            X = new Left(123)
        };

        var object2 = new
        {
            A = 2,
            B = "Not Quite Absolute",
            C = new[] { 3, 2, 1 },
            Float = 1.111_111_9m,
            Double = 1.111_111_111_111_119m,
            String = new Regex("^(abc)\\d+$"),
            Inner = new TestType
            {
                X = 1,
                Y = 3,
                Z = 3
            },
            Set = new HashSet<int> { 1, 2, 3, 5 },
            Dictionary = new Dictionary<int, int>
            {
                {123, 2345},
                {34, 456},
                {2, 3}
            },
            X = new Right(234)
        };

        var syntax = object1
            .WithDeepEqual(object2)
            .MapProperty<Left, Right>(x => x.Size, x => x.Width)
            .WithCustomComparison(new RegexComparison())
            .WithCustomFormatter<RegexDifference>(new RegexDifferenceFormatter());

        var exception = Assert.Throws<DeepEqualException>(() => syntax.Assert());

        Approvals.Verify(exception.Message);
    }

    [Fact]
    public void AssertOnTuple()
    {
        var left = new KeyValuePair<string, object>("1", new Model { Id = 10, Name = "1" });
        var right = new KeyValuePair<string, object>("1", new Model { Id = 10, Name = "1" });
        var left_struct = new KeyValuePair<string, int>("1", 1);
        var right_struct = new KeyValuePair<string, int>("1", 1);

        DeepAssert.AreEqual(left, right);
        DeepAssert.AreEqual(left_struct, right_struct);
    }

    public class Model
    {
        public int Id { get; set;}
        public string Name { get; set; }
    }
}

public class TestType
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
}

public record Left(int Size);
public record Right(long Width);

public class RegexComparison : IComparison
{
    public bool CanCompare(IComparisonContext context, Type leftType, Type rightType)
    {
        return leftType == typeof(string) && rightType == typeof(Regex);
    }

    public (ComparisonResult result, IComparisonContext context) Compare(
        IComparisonContext context,
        object leftValue,
        object rightValue
    )
    {
        var str = (string) leftValue;
        var regex = (Regex) rightValue;

        if (regex.IsMatch(str))
            return (ComparisonResult.Pass, context);

        return (
            ComparisonResult.Fail,
            context.AddDifference(new RegexDifference(context.Breadcrumb, str, regex))
        );
    }
}

public record RegexDifference(BreadcrumbPair Breadcrumb, string Value, Regex Regex) : Difference(Breadcrumb);

public class RegexDifferenceFormatter : IDifferenceFormatter
{
    public string Format(Difference difference)
    {
        var regexDiff = (RegexDifference) difference;

        return $"{regexDiff.Breadcrumb.Left} doesn't match regex {regexDiff.Regex}\n{regexDiff.Value}";
    }
}
