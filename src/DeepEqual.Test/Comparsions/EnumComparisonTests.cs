namespace DeepEqual.Test.Comparsions
{
	using System;
	using System.Collections.Generic;

	using DeepEqual;

	using AutoFixture;

	using Shouldly;

	using Xbehave;
	using Xunit;

	public class EnumComparisonTests
	{
		protected Fixture Fixture { get; set; }

		protected EnumComparison SUT { get; set; }
		protected ComparisonContext Context { get; set; }

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
		public void Can_compare_types(Type type1, Type type2, bool expected)
		{
			"Given a Fixture".x(() => 
				Fixture = new Fixture()
			);

			"And an EnumComparer".x(() => 
				SUT = Fixture.Create<EnumComparison>()
			);

			"When calling CanCompare({0}, {1})".x(() => 
				CanCompareResult = SUT.CanCompare(type1, type2)
			);

			"It should return {2}".x(() => 
				CanCompareResult.ShouldBe(expected)
			);
		}

		[Scenario]
		[MemberData(nameof(CompareTestData))]
		public void Comparing_values(object value1, object value2, ComparisonResult expected)
		{
			"Given a Fixture".x(() => 
				Fixture = new Fixture()
			);

			"And an EnumComparer".x(() => 
				SUT = Fixture.Create<EnumComparison>()
			);

			"And a Comparison context object".x(() =>
			{
				Context = new ComparisonContext("Property");
			});

			"When calling Compare({0}, {1})".x(() => 
				Result = SUT.Compare(Context, value1, value2)
			);

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
				var expectedDifference = new Difference
					{
						Breadcrumb = "Property",
						Value1 = value1,
						Value2 = value2
					};

				"And it should add a differences".x(() =>
					Context.Differences[0].ShouldBe(expectedDifference)
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

		public static IEnumerable<object[]> CompareTestData => new[]
		{
			new object[] {TestEnum1.A, TestEnum1.A, ComparisonResult.Pass},
			new object[] {TestEnum1.A, TestEnum2.A, ComparisonResult.Pass},
			new object[] {TestEnum1.A, TestEnum1.B, ComparisonResult.Fail},
			new object[] {TestEnum1.A, TestEnum2.B, ComparisonResult.Fail},
			new object[] {TestEnum1.B, TestEnum2.B, ComparisonResult.Pass},
			new object[] {TestEnum2.B, TestEnum2.B, ComparisonResult.Pass},
			new object[] {TestEnum1.A, 1, ComparisonResult.Pass},
			new object[] {TestEnum1.A, 2, ComparisonResult.Fail},
			new object[] {TestEnum1.A, "A", ComparisonResult.Pass},
			new object[] {TestEnum1.A, "AAA", ComparisonResult.Fail},
			new object[] {2, TestEnum1.B, ComparisonResult.Pass},
			new object[] {3, TestEnum1.B, ComparisonResult.Fail},
			new object[] {3, TestEnum2.B, ComparisonResult.Pass},
			new object[] {"B", TestEnum2.B, ComparisonResult.Pass}
		};

		public static IEnumerable<object[]> CanCompareTypesTestData => new[]
		{
			new object[] {typeof (TestEnum1), typeof (TestEnum1), true},
			new object[] {typeof (TestEnum1), typeof (TestEnum2), true},
			new object[] {typeof (object), typeof (object), false},
			new object[] {typeof (object), typeof (int), false},
			new object[] {typeof (int), typeof (int), false},
			new object[] {typeof (int), typeof (string), false},
			new object[] {typeof (TestEnum1), typeof (object), false},
			new object[] {typeof (TestEnum1), typeof (int), true},
			new object[] {typeof (TestEnum1), typeof (long), false},
			new object[] {typeof (TestEnum1), typeof (string), true},
			new object[] {typeof (object), typeof (TestEnum1), false},
			new object[] {typeof (int), typeof (TestEnum1), true},
			new object[] {typeof (long), typeof (TestEnum1), false},
			new object[] {typeof (string), typeof (TestEnum1), true}
		};
	}
}