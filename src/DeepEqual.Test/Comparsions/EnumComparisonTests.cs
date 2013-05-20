namespace DeepEqual.Test.Comparsions
{
	using System;
	using System.Collections.Generic;

	using DeepEqual;

	using Ploeh.AutoFixture;

	using Shouldly;

	using Xbehave;

	using Xunit.Extensions;

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
			"When creating a EnumComperer"
				.When(() => SUT = new EnumComparison());

			"it should implement IComparer"
				.Then(() => SUT.ShouldBeTypeOf<IComparison>());
		}

		[Scenario]
		[PropertyData("CanCompareTypesTestData")]
		public void Can_compare_types(Type type1, Type type2, bool expected)
		{
			"Given a Fixture"
				.Given(() => Fixture = new Fixture());

			"And an EnumComparer"
				.And(() => SUT = Fixture.Create<EnumComparison>());

			"When calling CanCompare({0}, {1})"
				.When(() => CanCompareResult = SUT.CanCompare(type1, type2));

			"It should return {2}"
				.Then(() => CanCompareResult.ShouldBe(expected));
		}

		[Scenario]
		[PropertyData("CompareTestData")]
		public void Comparing_values(object value1, object value2, ComparisonResult expected)
		{
			"Given a Fixture"
				.Given(() => Fixture = new Fixture());

			"And an EnumComparer"
				.And(() => SUT = Fixture.Create<EnumComparison>());

			"And a Comparison context object"
				.And(() =>
					{
						Context = new ComparisonContext("Property");
					});

			"When calling Compare({0}, {1})"
				.When(() => Result = SUT.Compare(Context, value1, value2));

			"Then it should return {2}"
				.Then(() => Result.ShouldBe(expected));

			if (expected == ComparisonResult.Pass)
			{
				"And it should not add any differences"
					.And(() => Context.Differences.Count.ShouldBe(0));
			}
			else
			{
				var expectedDifference = new Difference
					{
						Breadcrumb = "Property",
						Value1 = value1,
						Value2 = value2
					};

				"And it should add a differences"
					.And(() => Context.Differences[0].ShouldBe(expectedDifference));
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

		public static IEnumerable<object[]> CompareTestData
		{
			get
			{
				return new[]
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
			}
		}

		public static IEnumerable<object[]> CanCompareTypesTestData
		{
			get
			{
				return new[]
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
	}
}