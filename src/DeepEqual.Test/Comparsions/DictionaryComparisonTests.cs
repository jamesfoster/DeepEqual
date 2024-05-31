using System;
using System.Collections;
using System.Collections.Generic;

using DeepEqual.Test.Helper;

using AutoFixture;
using AutoFixture.AutoMoq;

using Shouldly;

using Xbehave;
using Xunit;

namespace DeepEqual.Test.Comparsions;

public class DictionaryComparisonTests
{
    protected Fixture Fixture { get; set; }

    protected DictionaryComparison SUT { get; set; }

    protected ComparisonContext Context { get; set; }

    protected ComparisonResult Result { get; set; }
    protected bool CanCompareResult { get; set; }

    private void SetUp()
    {
        IComparison inner = null!;

        "Given an inner comparison".x(() =>
            inner = new MockComparison()
        );

        "And an ListComparison".x(() =>
            SUT = new DictionaryComparison(keyComparer: new DefaultComparison(skippedTypes: []))
        );

        "And a ComparisonContext".x(() =>
            Context = new ComparisonContext(inner, new BreadcrumbPair("Set"))
        );
    }

    [Scenario]
    public void Creating_a_SetComparison()
    {
        "Given a fixture".x(() =>
        {
            Fixture = new Fixture();
            Fixture.Customize(new AutoMoqCustomization());
        });

        "When creating an SetComparison".x(() =>
            SUT = Fixture.Create<DictionaryComparison>()
        );

        "Then it should implement IComparison".x(() =>
            SUT.ShouldBeAssignableTo<IComparison>()
        );
    }

    [Scenario]
    [MemberData(nameof(CanCompareTypesTestData))]
    public void Can_compare_types(Type type1, Type type2, bool expected)
    {
        SetUp();

        "When calling CanCompare({0}, {1})".x(() =>
            CanCompareResult = SUT.CanCompare(Context, type1, type2)
        );

        "It should return {2}".x(() =>
            CanCompareResult.ShouldBe(expected)
        );
    }

    [Scenario]
    [MemberData(nameof(IntTestData))]
    public void When_comparing_sets(
        IDictionary leftValue,
        IDictionary rightValue,
        ComparisonResult expected,
        int numDifferences
    )
    {
        SetUp();

        var context = default (IComparisonContext);

        "When comparing dictionaries".x(() =>
            (Result, context) = SUT.Compare(Context, leftValue, rightValue)
        );

        "Then it should return {2}".x(() =>
            Result.ShouldBe(expected)
        );

        "And have {3} differences".x(() =>
            context.Differences.Count.ShouldBe(numDifferences)
        );
    }

    #region Test Data

    public static IEnumerable<object[]> IntTestData => new[]
    {
        new object[]
        {
            new Dictionary<int, int>(),
            new Dictionary<int, int>(),
            ComparisonResult.Pass,
            0
        },
        new object[]
        {
            new Dictionary<int, int> {{123, 456}, {234, 567}},
            new Dictionary<int, int> {{123, 456}, {234, 567}},
            ComparisonResult.Pass,
            0
        },
        new object[]
        {
            new Dictionary<int, int> {{234, 567}, {12, 23}, {123, 456}},
            new Dictionary<int, int> {{123, 456}, {234, 567}, {12, 23}},
            ComparisonResult.Pass,
            0
        },
        new object[]
        {
            new Dictionary<int, int> {{234, 567}, {12, 23}, {123, 456}},
            new SortedDictionary<int, int> {{123, 456}, {234, 567}, {12, 23}},
            ComparisonResult.Pass,
            0
        },

        new object[]
        {
            new Dictionary<int, int> {{1, 1}},
            new Dictionary<int, int>(),
            ComparisonResult.Fail,
            1
        },
        new object[]
        {
            new Dictionary<int, int> {{1, 1}},
            new Dictionary<int, int> {{2, 2}},
            ComparisonResult.Fail,
            1
        },
        new object[]
        {
            new Dictionary<int, int> {{1, 1}},
            new Dictionary<int, int> {{1, 1}, {2, 2}},
            ComparisonResult.Fail,
            1
        },
        new object[]
        {
            new Dictionary<int, int> {{1, 1}, {2, 2}},
            new Dictionary<int, int> {{1, 1}},
            ComparisonResult.Fail,
            1
        },
        new object[]
        {
            new Dictionary<int, int> {{1, 1}, {2, 2}, {3, 3}},
            new Dictionary<int, int> {{1, 2}, {2, 3}, {3, 4}},
            ComparisonResult.Fail,
            3
        }
    };

    public static IEnumerable<object[]> CanCompareTypesTestData =>
    [
        [typeof (Dictionary<int, int>), typeof (Dictionary<int, int>), true],
        [typeof (Dictionary<int, int>), typeof (SortedDictionary<int, int>), true],
        [typeof (IDictionary), typeof (Dictionary<object, object>), true],

        [typeof (List<int>), typeof (List<int>), false],
        [typeof (IEnumerable<int>), typeof (IEnumerable), false],
        [typeof (string), typeof (string), false],
        [typeof (object), typeof (object), false],
        [typeof (object), typeof (int), false],
        [typeof (int), typeof (int), false],
        [typeof (int), typeof (string), false]
    ];

    #endregion

}