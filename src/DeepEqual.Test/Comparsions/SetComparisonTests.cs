﻿using DeepEqual.Syntax;
using DeepEqual.Test.Helper;

using Shouldly;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Xbehave;

using Xunit;

namespace DeepEqual.Test.Comparsions;

public class SetComparisonTests
{
    [Scenario]
    public void Creating_a_SetComparison()
    {
        SetComparison SUT = null!;

        "When creating an SetComparison".x(() =>
            SUT = new SetComparison()
        );

        "Then it should implement IComparison".x(() =>
            SUT.ShouldBeAssignableTo<IComparison>()
        );
    }

    [Scenario]
    [MemberData(nameof(CanCompareTypesTestData))]
    public void Can_compare_types(Type type1, Type type2, Type elementType1, Type elementType2, bool expected)
    {
        SetComparison SUT = null!;

        MockComparison inner = null!;
        IComparisonContext context = null!;

        bool canCompareResult = false;

        "Given an inner comparison".x(() =>
        {
            inner = new MockComparison();
        });

        "And a Comparison context object".x(() =>
            context = new ComparisonContext(inner, new BreadcrumbPair("Set"))
        );

        "And an ListComparison".x(() =>
            SUT = new SetComparison()
        );

        "When calling CanCompare({0}, {1})".x(() =>
            canCompareResult = SUT.CanCompare(context, type1, type2)
        );

        "It should return {2}".x(() =>
            canCompareResult.ShouldBe(expected)
        );

        if (expected)
        {
            "and it should call CanCompare on the inner comparer".x(() =>
                inner.CanCompareCalls.ShouldContain((context, elementType1, elementType2))
            );
        }
    }

    [Scenario]
    [MemberData(nameof(IntTestData))]
    public void When_comparing_sets(IEnumerable leftValue, IEnumerable rightValue, ComparisonResult expected)
    {
        SetComparison SUT = null!;

        MockComparison inner = null!;
        IComparisonContext context = null!;
        ComparisonResult result = default;

        var leftList = leftValue.Cast<object>().ToArray();
        var rightList = rightValue.Cast<object>().ToArray();

        "Given an inner comparison".x(() =>
        {
            inner = new MockComparison();
        });

        "And a SetComparison".x(() =>
            SUT = new SetComparison()
        );

        "And a Comparison context object".x(() =>
            context = new ComparisonContext(inner, new BreadcrumbPair("Set"))
        );

        "When comparing enumerables".x(() =>
        {
            (result, context) = SUT.Compare(context, leftValue, rightValue);
        });

        "Then it should return right{}".x(() =>
            result.ShouldBe(expected)
        );

        if (leftList.Length == rightList.Length)
        {
            "And it should call the inner comparison Compare for each element in the set".x(() =>
            {
                for (var i = 0; i < leftList.Length; i++)
                {
                    var local = i;

                    inner.CompareCalls.ShouldContain(call => call.leftValue.Equals(leftList[local]));
                }
            });

            if (expected == ComparisonResult.Fail)
            {
                "And it should add a SetDifference".x(() =>
                    context.Differences[0].ShouldBeAssignableTo<SetDifference>()
                );
            }
        }
        else
        {
            var expectedDifference = new BasicDifference(
                Breadcrumb: new BreadcrumbPair("Set"),
                LeftValue: leftList.Length,
                RightValue: rightList.Length,
                LeftChildProperty: "Count",
                RightChildProperty: "Count"
            );

            "And it should add a Difference".x(() =>
                context.Differences[0].ShouldDeepEqual(expectedDifference)
            );
        }
    }

    public static IEnumerable<object[]> IntTestData => [
        [new HashSet<int>(),           new int[] {},               ComparisonResult.Pass],
        [new HashSet<int>(),           new List<int>(),            ComparisonResult.Pass],
        [new HashSet<int> {1},         new[] {1},                  ComparisonResult.Pass],
        [new HashSet<int> {1},         new[] {1},                  ComparisonResult.Pass],
        [new[] {1, 2, 3},              new HashSet<int> {1, 2, 3}, ComparisonResult.Pass],
        [new SortedSet<int> {1, 2, 3}, new[] {1, 2, 3},            ComparisonResult.Pass],

        [new HashSet<int> {1, 2, 3},   new[] {1, 3, 2},            ComparisonResult.Pass],
        [new HashSet<int> {3, 2, 1},   new[] {1, 2, 3},            ComparisonResult.Pass],
        [new HashSet<int> {3, 1, 2},   new[] {1, 3, 2},            ComparisonResult.Pass],

        [new HashSet<int> {1},         new[] {2},                  ComparisonResult.Fail],
        [new HashSet<int> {1},         new[] {1, 1},               ComparisonResult.Fail],
        [new HashSet<int> {1, 2, 3},   new[] {1, 3, 3},            ComparisonResult.Fail]
    ];

    public static IEnumerable<object[]> CanCompareTypesTestData => [
        [typeof (ISet<int>),        typeof (ISet<int>),        typeof (int),    typeof (int),    true],
        [typeof (ISet<int>),        typeof (IList),            typeof (int),    typeof (object), true],
        [typeof (ISet<object>),     typeof (IList<int>),       typeof (object), typeof (int),    true],
        [typeof (ISet<object>),     typeof (List<int>),        typeof (object), typeof (int),    true],
        [typeof (ISet<object>),     typeof (IEnumerable<int>), typeof (object), typeof (int),    true],

        [typeof (List<int>),        typeof (List<int>),        typeof (int),    typeof (int),    false],
        [typeof (IEnumerable<int>), typeof (IEnumerable),      typeof (int),    typeof (object), false],
        [typeof (string),           typeof (string),           typeof (char),   typeof (char),   false],
        [typeof (ISet<int>),        typeof (int),              null,            null,            false],
        [typeof (int),              typeof (ISet<int>),        null,            null,            false],
        [typeof (object),           typeof (object),           null,            null,            false],
        [typeof (object),           typeof (int),              null,            null,            false],
        [typeof (int),              typeof (int),              null,            null,            false],
        [typeof (int),              typeof (string),           null,            null,            false]
    ];
}