namespace DeepEquals.Test.Comparsions
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Globalization;
	using System.Linq;

	using DeepEquals;

	using Moq;

	using Ploeh.AutoFixture;
	using Ploeh.AutoFixture.AutoMoq;

	using Xbehave;

	using Shouldly;

	using Xunit.Extensions;

	public class ListComparisonTests
	{
		protected Fixture Fixture { get; set; }

		protected ListComparison SUT { get; set; }

		protected Mock<IComparison> Inner { get; set; }
		protected ComparisonContext Context { get; set; }
		protected IDictionary<string, Mock<IComparisonContext>> IndexContexts { get; set; }

		protected ComparisonResult Result { get; set; }
		protected bool CanCompareResult { get; set; }

		[Scenario]
		public void Creating_an_EnumerableComparison()
		{
			"Given a fixture"
				.Given(() =>
					{
						Fixture = new Fixture();
						Fixture.Customize(new AutoMoqCustomization());
					});

			"When creating an ListComparison"
				.When(() => SUT = Fixture.Create<ListComparison>());

			"Then it should implement IComparison"
				.Then(() => SUT.ShouldBeTypeOf<IComparison>());
		}

		[Scenario]
		[PropertyData("IntTestData")]
		public void When_comparing_enumerables(IEnumerable value1, IEnumerable value2, ComparisonResult expected)
		{
			var list1 = value1.Cast<object>().ToArray();
			var list2 = value2.Cast<object>().ToArray();

			"Given a fixture"
				.Given(() =>
					{
						Fixture = new Fixture();
						Fixture.Customize(new AutoMoqCustomization());
					});

			"And an inner comparison"
				.And(() =>
					{
						Inner = Fixture.Freeze<Mock<IComparison>>();

						Inner
							.Setup(x => x.CanCompare(It.IsAny<Type>(), It.IsAny<Type>()))
							.Returns(true);

						Inner
							.Setup(x => x.Compare(It.IsAny<IComparisonContext>(), It.IsAny<object>(), It.IsAny<object>()))
							.Returns<IComparisonContext, object, object>((c, v1, v2) => v1.Equals(v2) ? ComparisonResult.Pass : ComparisonResult.Fail);
					});

			"And a ListComparison"
				.And(() => SUT = Fixture.Create<ListComparison>());

			"And a Comparison context object"
				.And(() => Context = new ComparisonContext("List"));

			"When comparing enumerables"
				.When(() => Result = SUT.Compare(Context, value1, value2));

			"Then it should return {2}"
				.Then(() => Result.ShouldBe(expected));

			if (list1.Length == list2.Length)
			{
				"And it should call the inner comparison Compare for each element in the list"
					.And(() =>
						{
							var pairs = list1.Zip(list2, Tuple.Create).ToList();
							for (var i = 0; i < pairs.Count; i++)
							{
								var p = pairs[i];
								Inner.Verify(x => x.Compare(
									It.Is<IComparisonContext>(c => c.Breadcrumb == "List[" + i + "]"),
									p.Item1,
									p.Item2), Times.Once());
							}
						});
			}
			else
			{
				var expectedDifference = new Difference
					{
						Breadcrumb = "List",
						ChildProperty = "Count",
						Value1 = list1.Length,
						Value2 = list2.Length
					};

				"And it should add a Difference"
					.And(() => Context.Differences[0].ShouldBe(expectedDifference));
			}
		}

		[Scenario]
		[PropertyData("CanCompareTypesTestData")]
		public void Can_compare_types(Type type1, Type type2, Type elementType1, Type elementType2, bool expected)
		{
			"Given a fixture"
				.Given(() =>
					{
						Fixture = new Fixture();
						Fixture.Customize(new AutoMoqCustomization());
					});

			"And an inner comparison"
				.And(() =>
					{
						Inner = Fixture.Freeze<Mock<IComparison>>();

						Inner
							.Setup(x => x.CanCompare(It.IsAny<Type>(), It.IsAny<Type>()))
							.Returns(true);
					});

			"And an ListComparison"
				.And(() => SUT = Fixture.Create<ListComparison>());

			"When calling CanCompare({0}, {1})"
				.When(() => CanCompareResult = SUT.CanCompare(type1, type2));

			"It should return {2}"
				.Then(() => CanCompareResult.ShouldBe(expected));

			if (expected)
			{
				"and it should call CanCompare on the inner comparer"
					.And(() => Inner.Verify(x => x.CanCompare(elementType1, elementType2)));
			}
		}

		public static IEnumerable<object[]> IntTestData
		{
			get
			{
				return new[]
					{
						new object[] {new List<int>(), new int[] {}, ComparisonResult.Pass},
						new object[] {new List<int>(), new List<int>(), ComparisonResult.Pass},
						new object[] {new List<int> {1}, new[] {1}, ComparisonResult.Pass},
						new object[] {new List<int> {1}, new[] {1}, ComparisonResult.Pass},
						new object[] {new[] {1, 2, 3}, new List<int> {1, 2, 3}, ComparisonResult.Pass},
						new object[] {new List<int> {1, 2, 3}, new[] {1, 2, 3}, ComparisonResult.Pass},
						new object[] {new Collection<int> {1, 2, 3}, new[] {1, 2, 3}, ComparisonResult.Pass},
						new object[] {Enumerate(1, 2, 3), new[] {1, 2, 3}, ComparisonResult.Pass},

						new object[] {new List<int> {1}, new[] {2}, ComparisonResult.Fail},
						new object[] {new List<int> {1}, new[] {1, 1}, ComparisonResult.Fail},
						new object[] {new List<int> {1, 2, 3}, new[] {1, 2, 2}, ComparisonResult.Fail}
					};
			}
		}

		private static IEnumerable<T> Enumerate<T>(params T[] values)
		{
			foreach (var value in values)
			{
				yield return value;
			}
		}

		public static IEnumerable<object[]> CanCompareTypesTestData
		{
			get
			{
				return new[]
					{
						new object[] {typeof (IList), typeof (IList), typeof (object), typeof (object), true},
						new object[] {typeof (IList<int>), typeof (IList<int>), typeof (int), typeof (int), true},
						new object[] {typeof (List<int>), typeof (List<int>), typeof (int), typeof (int), true},
						new object[] {typeof (IEnumerable<int>), typeof (IEnumerable), typeof (int), typeof (object), true},
						new object[] {typeof (string), typeof (string), typeof (char), typeof (char), true},

						new object[] {typeof (object), typeof (object), null, null, false},
						new object[] {typeof (object), typeof (int), null, null, false},
						new object[] {typeof (int), typeof (int), null, null, false},
						new object[] {typeof (int), typeof (string), null, null, false}
					};
			}
		}
	}
}