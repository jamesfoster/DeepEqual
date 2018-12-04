namespace DeepEqual.Test.Comparsions
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	using Moq;

	using AutoFixture;
	using AutoFixture.AutoMoq;

	using Shouldly;

	using Xbehave;
	using Xunit;

	public class SetComparisonTests
	{
		protected Fixture Fixture { get; set; }

		protected SetComparison SUT { get; set; }

		protected Mock<IComparison> Inner { get; set; }
		protected ComparisonContext Context { get; set; }
		protected IDictionary<string, Mock<IComparisonContext>> IndexContexts { get; set; }

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
				Fixture.Customize(new AutoMoqCustomization());
			});

			"And an inner comparison".x(() =>
			{
				Inner = Fixture.Freeze<Mock<IComparison>>();

				Inner
					.Setup(x => x.CanCompare(It.IsAny<Type>(), It.IsAny<Type>()))
					.Returns(true);
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
					Inner.Verify(x => x.CanCompare(elementType1, elementType2))
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
				Fixture.Customize(new AutoMoqCustomization());
			});

			"And an inner comparison".x(() =>
			{
				Inner = Fixture.Freeze<Mock<IComparison>>();

				Inner
					.Setup(x => x.CanCompare(It.IsAny<Type>(), It.IsAny<Type>()))
					.Returns(true);

				Inner
					.Setup(x => x.Compare(It.IsAny<IComparisonContext>(), It.IsAny<object>(), It.IsAny<object>()))
					.Returns<IComparisonContext, object, object>((c, v1, v2) => v1.Equals(v2) ? ComparisonResult.Pass : ComparisonResult.Fail);
			});

			"And a SetComparison".x(() => 
				SUT = Fixture.Create<SetComparison>()
			);

			"And a Comparison context object".x(() => 
				Context = new ComparisonContext("Set")
			);

			"When comparing enumerables".x(() => 
				Result = SUT.Compare(Context, value1, value2)
			);

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

						Inner.Verify(x => x.Compare(
							It.IsAny<ComparisonContext>(),
							list1[local],
							It.IsAny<object>()), Times.AtLeastOnce());
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
				var expectedDifference = new Difference
					{
						Breadcrumb = "Set",
						ChildProperty = "Count",
						Value1 = list1.Length,
						Value2 = list2.Length
					};

				"And it should add a Difference".x(() =>
					Context.Differences[0].ShouldBe(expectedDifference)
				);
			}
		}

		public static IEnumerable<object[]> IntTestData => new[]
		{
			new object[] {new HashSet<int>(), new int[] {}, ComparisonResult.Pass},
			new object[] {new HashSet<int>(), new List<int>(), ComparisonResult.Pass},
			new object[] {new HashSet<int> {1}, new[] {1}, ComparisonResult.Pass},
			new object[] {new HashSet<int> {1}, new[] {1}, ComparisonResult.Pass},
			new object[] {new[] {1, 2, 3}, new HashSet<int> {1, 2, 3}, ComparisonResult.Pass},
			new object[] {new SortedSet<int> {1, 2, 3}, new[] {1, 2, 3}, ComparisonResult.Pass},
						
			new object[] {new HashSet<int> {1, 2, 3}, new[] {1, 3, 2}, ComparisonResult.Pass},
			new object[] {new HashSet<int> {3, 2, 1}, new[] {1, 2, 3}, ComparisonResult.Pass},
			new object[] {new HashSet<int> {3, 1, 2}, new[] {1, 3, 2}, ComparisonResult.Pass},

			new object[] {new HashSet<int> {1}, new[] {2}, ComparisonResult.Fail},
			new object[] {new HashSet<int> {1}, new[] {1, 1}, ComparisonResult.Fail},
			new object[] {new HashSet<int> {1, 2, 3}, new[] {1, 3, 3}, ComparisonResult.Fail}
		};

		public static IEnumerable<object[]> CanCompareTypesTestData => new[]
		{
			new object[] {typeof (ISet<int>), typeof (ISet<int>), typeof (int), typeof (int), true},
			new object[] {typeof (ISet<int>), typeof (IList), typeof (int), typeof (object), true},
			new object[] {typeof (ISet<object>), typeof (IList<int>), typeof (object), typeof (int), true},
			new object[] {typeof (ISet<object>), typeof (List<int>), typeof (object), typeof (int), true},
			new object[] {typeof (ISet<object>), typeof (IEnumerable<int>), typeof (object), typeof (int), true},

			new object[] {typeof (List<int>), typeof (List<int>), typeof (int), typeof (int), false},
			new object[] {typeof (IEnumerable<int>), typeof (IEnumerable), typeof (int), typeof (object), false},
			new object[] {typeof (string), typeof (string), typeof (char), typeof (char), false},
			new object[] {typeof (object), typeof (object), null, null, false},
			new object[] {typeof (object), typeof (int), null, null, false},
			new object[] {typeof (int), typeof (int), null, null, false},
			new object[] {typeof (int), typeof (string), null, null, false}
		};
	}
}