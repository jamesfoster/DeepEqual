namespace DeepEqual.Test.Comparsions
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	using DeepEqual.Test.Helper;

	using AutoFixture;
	using AutoFixture.AutoMoq;

	using Shouldly;

	using Xbehave;
	using Xunit;

	public class DictionaryComparisonTests
	{
		protected Fixture Fixture { get; set; }

		protected DictionaryComparison SUT { get; set; }

		protected ComparisonContext Context { get; set; }

		protected ComparisonResult Result { get; set; }
		protected bool CanCompareResult { get; set; }

		private void SetUp()
		{
			"Given a fixture".x(() =>
				Fixture = new Fixture()
			);

			"And an inner comparison".x(() =>
				Fixture.Register<IComparison>(() => new MockComparison())
			);

			"And an ListComparison".x(() => 
				SUT = Fixture.Create<DictionaryComparison>()
			);

			"And a ComparisonContext".x(() => 
				Context = new ComparisonContext("Set")
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
				CanCompareResult = SUT.CanCompare(type1, type2)
			);

			"It should return {2}".x(() => 
				CanCompareResult.ShouldBe(expected)
			);
		}

		[Scenario]
		[MemberData(nameof(IntTestData))]
		public void When_comparing_sets(IDictionary value1, IDictionary value2, ComparisonResult expected)
		{
			SetUp();

			"When comparing dictionaries".x(() =>
				(Result, _) = SUT.Compare(Context, value1, value2)
			);

			"Then it should return {2}".x(() => 
				Result.ShouldBe(expected)
			);
		}

		#region Test Data

		public static IEnumerable<object[]> IntTestData => new[]
		{
			new object[]
			{
				new Dictionary<int, int>(),
				new Dictionary<int, int>(),
				ComparisonResult.Pass
			},
			new object[]
			{
				new Dictionary<int, int> {{123, 456}, {234, 567}},
				new Dictionary<int, int> {{123, 456}, {234, 567}},
				ComparisonResult.Pass
			},
			new object[]
			{
				new Dictionary<int, int> {{234, 567}, {12, 23}, {123, 456}},
				new Dictionary<int, int> {{123, 456}, {234, 567}, {12, 23}},
				ComparisonResult.Pass
			},
			new object[]
			{
				new Dictionary<int, int> {{234, 567}, {12, 23}, {123, 456}},
				new SortedDictionary<int, int> {{123, 456}, {234, 567}, {12, 23}},
				ComparisonResult.Pass
			},

			new object[]
			{
				new Dictionary<int, int> {{1, 1}},
				new Dictionary<int, int>(),
				ComparisonResult.Fail
			},
			new object[]
			{
				new Dictionary<int, int> {{1, 1}},
				new Dictionary<int, int> {{2, 2}},
				ComparisonResult.Fail
			},
			new object[]
			{
				new Dictionary<int, int> {{1, 1}},
				new Dictionary<int, int> {{1, 1}, {2, 2}},
				ComparisonResult.Fail
			},
			new object[]
			{
				new Dictionary<int, int> {{1, 1}, {2, 2}},
				new Dictionary<int, int> {{1, 1}},
				ComparisonResult.Fail
			}
		};

		public static IEnumerable<object[]> CanCompareTypesTestData => new[]
		{
			new object[] {typeof (Dictionary<int, int>), typeof (Dictionary<int, int>), true},
			new object[] {typeof (Dictionary<int, int>), typeof (SortedDictionary<int, int>), true},
			new object[] {typeof (IDictionary), typeof (Dictionary<object, object>), true},

			new object[] {typeof (List<int>), typeof (List<int>), false},
			new object[] {typeof (IEnumerable<int>), typeof (IEnumerable), false},
			new object[] {typeof (string), typeof (string), false},
			new object[] {typeof (object), typeof (object), false},
			new object[] {typeof (object), typeof (int), false},
			new object[] {typeof (int), typeof (int), false},
			new object[] {typeof (int), typeof (string), false}
		};

		#endregion

	}
}