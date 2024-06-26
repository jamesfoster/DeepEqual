﻿using DeepEqual.Syntax;
using DeepEqual.Test.Helper;

using Moq;

using Shouldly;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Xbehave;

using Xunit;

namespace DeepEqual.Test.Comparsions;

public class ListComparisonTests
{
    protected ListComparison SUT { get; set; }

    protected MockComparison Inner { get; set; }
    protected IComparisonContext Context { get; set; }
    protected IDictionary<string, Mock<IComparisonContext>> IndexContexts { get; set; }

    protected ComparisonResult Result { get; set; }
    protected bool CanCompareResult { get; set; }

    [Scenario]
    public void Creating_an_EnumerableComparison()
    {
        "When creating an ListComparison".x(() =>
            SUT = new ListComparison()
        );

        "Then it should implement IComparison".x(() =>
            SUT.ShouldBeAssignableTo<IComparison>()
        );
    }

    [Scenario]
    [MemberData(nameof(IntTestData))]
    public void When_comparing_enumerables(
        IEnumerable leftValue,
        IEnumerable rightValue,
        ComparisonResult expected
    )
    {
        var list1 = leftValue.Cast<object>().ToArray();
        var list2 = rightValue.Cast<object>().ToArray();

        "Given an inner comparison".x(() =>
        {
            Inner = new MockComparison();
        });

        "And a ListComparison".x(() =>
            SUT = new ListComparison()
        );

        "And a Comparison context object".x(() =>
            Context = new ComparisonContext(Inner, new BreadcrumbPair("List"))
        );

        "When comparing enumerables".x(() =>
        {
			(var result, var context) = SUT.Compare(Context, leftValue, rightValue);
            Result = result;
            Context = context;
        });

        "Then it should return {2}".x(() =>
            Result.ShouldBe(expected)
        );

        if (list1.Length == list2.Length)
        {
            "And it should call the inner comparison Compare for each element in the list".x(() =>
            {
                var pairs = list1.Zip(list2, Tuple.Create).ToList();
                for (var i = 0; i < pairs.Count; i++)
                {
                    var p = pairs[i];
                    var index = i;

                    Inner.CompareCalls.ShouldContain(call =>
                        call.context.Breadcrumb.Left == $"List[{index}]" &&
                        call.context.Breadcrumb.Right == $"List[{index}]" &&
                        call.leftValue.Equals(p.Item1) &&
                        call.rightValue.Equals(p.Item2)
                    );
                }
            });
        }
        else
        {
            var expectedDifference = new
                {
                    Breadcrumb = new BreadcrumbPair("List"),
                    LeftChildProperty = "Count",
                    RightChildProperty = "Count",
                    LeftValue = list1.Length,
                    RightValue = list2.Length
                };

            "And it should add a Difference".x(() =>
                Context.Differences[0].ShouldDeepEqual(expectedDifference)
            );
        }
    }

    [Scenario]
    [MemberData(nameof(CanCompareTypesTestData))]
    public void Can_compare_types(Type type1, Type type2, Type elementType1, Type elementType2, bool expected)
    {
        "Given an inner comparison".x(() =>
        {
            Inner = new MockComparison();
        });

        "And an ListComparison".x(() =>
            SUT = new ListComparison()
        );

        "And a Comparison context object".x(() =>
            Context = new ComparisonContext(Inner, new BreadcrumbPair("List"))
        );

        "When calling CanCompare({0}, {1})".x(() =>
            CanCompareResult = SUT.CanCompare(Context, type1, type2)
        );

        "It should return {2}".x(() =>
            CanCompareResult.ShouldBe(expected)
        );

        if (expected)
        {
            "and it should call CanCompare on the inner comparer".x(() =>
                Inner.CanCompareCalls.ShouldContain(
                    call => call.leftType == elementType1 && call.rightType == elementType2
                )
            );
        }
    }

    public static IEnumerable<object[]> IntTestData =>
    [
        [new List<int>(), new int[] {}, ComparisonResult.Pass],
        [new List<int>(), new List<int>(), ComparisonResult.Pass],
        [new List<int> {1}, new[] {1}, ComparisonResult.Pass],
        [new List<int> {1}, new[] {1}, ComparisonResult.Pass],
        [new[] {1, 2, 3}, new List<int> {1, 2, 3}, ComparisonResult.Pass],
        [new List<int> {1, 2, 3}, new[] {1, 2, 3}, ComparisonResult.Pass],
        [new Collection<int> {1, 2, 3}, new[] {1, 2, 3}, ComparisonResult.Pass],
        [Enumerate(1, 2, 3), new[] {1, 2, 3}, ComparisonResult.Pass],

        [new List<int> {1}, new[] {2}, ComparisonResult.Fail],
        [new List<int> {1}, new[] {1, 1}, ComparisonResult.Fail],
        [new List<int> {1, 2, 3}, new[] {1, 2, 2}, ComparisonResult.Fail]
    ];

    private static IEnumerable<T> Enumerate<T>(params T[] values)
    {
        foreach (var value in values)
        {
            yield return value;
        }
    }

    public static IEnumerable<object[]> CanCompareTypesTestData =>
    [
        [typeof (IList), typeof (IList), typeof (object), typeof (object), true],
        [typeof (IList<int>), typeof (IList<int>), typeof (int), typeof (int), true],
        [typeof (List<int>), typeof (List<int>), typeof (int), typeof (int), true],
        [typeof (IEnumerable<int>), typeof (IEnumerable), typeof (int), typeof (object), true],
        [typeof (string), typeof (string), typeof (char), typeof (char), true],

        [typeof (object), typeof (object), null, null, false],
        [typeof (object), typeof (int), null, null, false],
        [typeof (int), typeof (int), null, null, false],
        [typeof (int), typeof (string), null, null, false]
    ];
}