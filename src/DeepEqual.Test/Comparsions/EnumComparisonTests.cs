using System;
using System.Collections.Generic;

using DeepEqual;
using DeepEqual.Syntax;

using AutoFixture;

using Shouldly;

using Xbehave;
using Xunit;

namespace DeepEqual.Test.Comparsions;

public class EnumComparisonTests
{
    protected Fixture Fixture { get; set; }

    protected EnumComparison SUT { get; set; }
    protected IComparisonContext Context { get; set; }

    protected ComparisonResult Result { get; set; }
    protected bool CanCompareResult { get; set; }

    [Scenario]
    public void Creating_an_EnumComparer()
    {
        "When creating a EnumComperer".x(() =>
            SUT = new EnumComparison()
        );

        "it should implement IComparer".x(() =>
            SUT.ShouldBeAssignableTo<IComparison>()
        );
    }

    [Scenario]
    [MemberData(nameof(CanCompareTypesTestData))]
    public void Can_compare_types(Type leftType, Type rightType, bool expected)
    {
        "Given a Fixture".x(() =>
            Fixture = new Fixture()
        );

        "And an EnumComparer".x(() =>
            SUT = Fixture.Create<EnumComparison>()
        );

        "When calling CanCompare({0}, {1})".x(() =>
            CanCompareResult = SUT.CanCompare(Context, leftType, rightType)
        );

        "It should return {2}".x(() =>
            CanCompareResult.ShouldBe(expected)
        );
    }

    [Scenario]
    [MemberData(nameof(CompareTestData))]
    public void Comparing_values(object leftValue, object rightValue, ComparisonResult expected)
    {
        "Given a Fixture".x(() =>
            Fixture = new Fixture()
        );

        "And an EnumComparer".x(() =>
            SUT = Fixture.Create<EnumComparison>()
        );

        "And a Comparison context object".x(() =>
        {
            Context = new ComparisonContext(rootComparison: null!, new BreadcrumbPair("Property"));
        });

        "When calling Compare({0}, {1})".x(() =>
        {
            (var result, var context) = SUT.Compare(Context, leftValue, rightValue);
            Result = result;
            Context = context;
        });

        "Then it should return {2}".x(() =>
            Result.ShouldBe(expected)
        );

        if (expected == ComparisonResult.Pass)
        {
            "And it should not add any differences".x(() =>
                Context.Differences.Count.ShouldBe(0)
            );
        }
        else
        {
            var expectedDifference = new BasicDifference(
                Breadcrumb: new BreadcrumbPair("Property"),
                LeftValue: leftValue,
                RightValue: rightValue,
                LeftChildProperty: null,
                RightChildProperty: null
            );

            "And it should add a differences".x(() =>
                Context.Differences[0].ShouldDeepEqual(expectedDifference)
            );
        }
    }

    private enum TestEnum1
    {
        A = 1,
        B = 2
    }

    private enum TestEnum2
    {
        A = 1,
        B = 3
    }

    public static IEnumerable<object[]> CompareTestData =>
    [
        [TestEnum1.A, TestEnum1.A, ComparisonResult.Pass],
        [TestEnum1.A, TestEnum2.A, ComparisonResult.Pass],
        [TestEnum1.A, TestEnum1.B, ComparisonResult.Fail],
        [TestEnum1.A, TestEnum2.B, ComparisonResult.Fail],
        [TestEnum1.B, TestEnum2.B, ComparisonResult.Pass],
        [TestEnum2.B, TestEnum2.B, ComparisonResult.Pass],
        [TestEnum1.A, 1, ComparisonResult.Pass],
        [TestEnum1.A, 2, ComparisonResult.Fail],
        [TestEnum1.A, "A", ComparisonResult.Pass],
        [TestEnum1.A, "AAA", ComparisonResult.Fail],
        [2, TestEnum1.B, ComparisonResult.Pass],
        [3, TestEnum1.B, ComparisonResult.Fail],
        [3, TestEnum2.B, ComparisonResult.Pass],
        ["B", TestEnum2.B, ComparisonResult.Pass]
    ];

    public static IEnumerable<object[]> CanCompareTypesTestData =>
    [
        [typeof (TestEnum1), typeof (TestEnum1), true],
        [typeof (TestEnum1), typeof (TestEnum2), true],
        [typeof (object), typeof (object), false],
        [typeof (object), typeof (int), false],
        [typeof (int), typeof (int), false],
        [typeof (int), typeof (string), false],
        [typeof (TestEnum1), typeof (object), false],
        [typeof (TestEnum1), typeof (int), true],
        [typeof (TestEnum1), typeof (long), false],
        [typeof (TestEnum1), typeof (string), true],
        [typeof (object), typeof (TestEnum1), false],
        [typeof (int), typeof (TestEnum1), true],
        [typeof (long), typeof (TestEnum1), false],
        [typeof (string), typeof (TestEnum1), true]
    ];
}