using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DeepEqual.Syntax;
using DeepEqual.Test.Helper;

using AutoFixture;
using AutoFixture.AutoMoq;

using Shouldly;

using Xbehave;
using Xunit;

namespace DeepEqual.Test.Comparsions;

public class SetComparisonTests
{
	protected Fixture Fixture { get; set; }

	protected SetComparison SUT { get; set; }

	protected MockComparison Inner { get; set; }
	protected IComparisonContext Context { get; set; }

	protected ComparisonResult Result { get; set; }
	protected bool CanCompareResult { get; set; }

	[Scenario]
	public void Creating_a_SetComparison()
	{
		"Given a fixture".x(() =>
		{
			Fixture = new Fixture();
			Fixture.Customize(new AutoMoqCustomization());
		});

		"When creating an SetComparison".x(() =>
			SUT = Fixture.Create<SetComparison>()
		);

		"Then it should implement IComparison".x(() =>
			SUT.ShouldBeAssignableTo<IComparison>()
		);
	}

	[Scenario]
	[MemberData(nameof(CanCompareTypesTestData))]
	public void Can_compare_types(Type type1, Type type2, Type elementType1, Type elementType2, bool expected)
	{
		"Given a fixture".x(() =>
		{
			Fixture = new Fixture();
		});

		"And an inner comparison".x(() =>
		{
			Inner = new MockComparison();
			Fixture.Inject<IComparison>(Inner);
		});

		"And an ListComparison".x(() =>
			SUT = Fixture.Create<SetComparison>()
		);

		"When calling CanCompare({0}, {1})".x(() =>
			CanCompareResult = SUT.CanCompare(type1, type2)
		);

		"It should return {2}".x(() =>
			CanCompareResult.ShouldBe(expected)
		);

		if (expected)
		{
			"and it should call CanCompare on the inner comparer".x(() =>
				Inner.CanCompareCalls.ShouldContain((elementType1, elementType2))
			);
		}
	}

	[Scenario]
	[MemberData(nameof(IntTestData))]
	public void When_comparing_sets(IEnumerable value1, IEnumerable value2, ComparisonResult expected)
	{
		var list1 = value1.Cast<object>().ToArray();
		var list2 = value2.Cast<object>().ToArray();

		"Given a fixture".x(() =>
		{
			Fixture = new Fixture();
		});

		"And an inner comparison".x(() =>
		{
			Inner = new MockComparison();
			Fixture.Inject<IComparison>(Inner);
		});

		"And a SetComparison".x(() =>
			SUT = Fixture.Create<SetComparison>()
		);

		"And a Comparison context object".x(() =>
			Context = new ComparisonContext(new BreadcrumbPair("Set"))
		);

		"When comparing enumerables".x(() =>
		{
			var (result, context) = SUT.Compare(Context, value1, value2);
			Result = result;
			Context = context;
		});

		"Then it should return {2}".x(() =>
			Result.ShouldBe(expected)
		);

		if (list1.Length == list2.Length)
		{
			"And it should call the inner comparison Compare for each element in the set".x(() =>
			{
				for (var i = 0; i < list1.Length; i++)
				{
					var local = i;

					Inner.CompareCalls.ShouldContain(call => call.value1.Equals(list1[local]));
				}
			});

			if (expected == ComparisonResult.Fail)
			{
				"And it should add a SetDifference".x(() =>
					Context.Differences[0].ShouldBeAssignableTo<SetDifference>()
				);
			}
		}
		else
		{
			var expectedDifference = new BasicDifference(
				Breadcrumb: new BreadcrumbPair("Set"),
				Value1: list1.Length,
				Value2: list2.Length,
				LeftChildProperty: "Count",
				RightChildProperty: "Count"
			);

			"And it should add a Difference".x(() =>
				Context.Differences[0].ShouldDeepEqual(expectedDifference)
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