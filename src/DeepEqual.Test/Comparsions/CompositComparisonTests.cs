namespace DeepEqual.Test.Comparsions
{
	using AutoFixture;
	using AutoFixture.AutoMoq;
	using DeepEqual;
	using DeepEqual.Test.Helper;
	using Moq;
	using Shouldly;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Xbehave;

	public class CompositComparisonTests
	{
		protected Fixture Fixture { get; set; }

		protected CompositeComparison SUT { get; set; }
		protected List<Mock<IComparison>> Inner { get; set; }
		protected Mock<IComparisonContext> Context { get; set; }

		protected ComparisonResult Result { get; set; }

		[Background]
		public void SetUp()
		{
			"Given a fixture".x(() =>
			{
				Fixture = new Fixture();
				Fixture.Customize(new AutoMoqCustomization());
			});

			"And some mock comparers".x(() =>
			{
				Inner = Fixture.Freeze<IEnumerable<IComparison>>()
								.Select(Mock.Get)
								.ToList();

				Inner.ForEach(
					m => m.Setup(c => c.CanCompare(It.IsAny<Type>(), It.IsAny<Type>()))
							.Returns(false));

				Inner.ForEach(
					m => m.Setup(c => c.Compare(It.IsAny<IComparisonContext>(), It.IsAny<object>(), It.IsAny<object>()))
							.Returns(ComparisonResult.Inconclusive));
			});

			"And a CompositeComparer".x(() =>
				SUT = Fixture
					.Build<CompositeComparison>()
					.With(x => x.Comparisons)
					.Create()
			);
		}

		[Scenario]
		public void When_creating_a_CompositeComparer()
		{
			"When creating a CompostieComperer".x(() =>
				SUT = new CompositeComparison()
			);

			"it should implement IComparer".x(() =>
				SUT.ShouldBeAssignableTo<IComparison>()
			);

			"CanCompare should always true".x(() =>
				SUT.CanCompare(null, null).ShouldBe(true)
			);
		}

		[Scenario]
		public void When_adding_a_caomparison()
		{
			"Given a CompostieComperer".x(() =>
				SUT = new CompositeComparison()
			);

			"When adding a comparer".x(() =>
				SUT.Add(Mock.Of<IComparison>())
			);

			"Then there should be one comparison".x(() =>
				SUT.Comparisons.Count.ShouldBe(1)
			);
		}

		[Scenario]
		public void When_testing_equality_if_a_comparer_returns_Inconclusive(object value1, object value2)
		{
			"Given the first comparer can compare the values".x(() => 
				Inner[0]
					.Setup(c => c.CanCompare(It.IsAny<Type>(), It.IsAny<Type>()))
					.Returns(true)
			);

			"... and returns Inconclusive".x(() => 
				Inner[0]
					.Setup(c => c.Compare(It.IsAny<IComparisonContext>(), It.IsAny<object>(), It.IsAny<object>()))
					.Returns(ComparisonResult.Inconclusive)
			);

			"And some values to compare".x(() =>
			{
				value1 = new object();
				value2 = new object();
			});

			"And a Comparison context object".x(() =>
				Context = Fixture.Create<Mock<IComparisonContext>>()
			);

			"When calling Compare".x(() =>
				Result = SUT.Compare(Context.Object, value1, value2)
			);

			"Then it should call CanCompare on all inner comparisons".x(() =>
				Inner.VerifyAll(c => c.CanCompare(typeof(object), typeof(object)), Times.Once())
			);

			"and it should call Compare on the first inner comparison".x(() =>
				Inner[0].Verify(c => c.Compare(Context.Object, value1, value2), Times.Once())
			);

			"but it shouldn't call the other comparisons Compare".x(() =>
				Inner.Skip(1).VerifyAll(c => c.Compare(It.IsAny<IComparisonContext>(), It.IsAny<object>(), It.IsAny<object>()), Times.Never())
			);

			"and it should return Inconclusive".x(() =>
				Result.ShouldBe(ComparisonResult.Inconclusive)
			);
		}

		[Scenario]
		public void When_testing_equality_if_a_comparer_returns_Pass(object value1, object value2)
		{
			"Given the first comparer can compare the values".x(() => 
				Inner[0]
					.Setup(c => c.CanCompare(It.IsAny<Type>(), It.IsAny<Type>()))
					.Returns(true)
			);

			"... and returns Pass".x(() => 
				Inner[0]
					.Setup(c => c.Compare(It.IsAny<IComparisonContext>(), It.IsAny<object>(), It.IsAny<object>()))
					.Returns(ComparisonResult.Pass)
			);

			"And some values to compare".x(() =>
			{
				value1 = new object();
				value2 = new object();
			});

			"And a Comparison context object".x(() =>
				Context = Fixture.Create<Mock<IComparisonContext>>()
			);

			"When calling Compare".x(() =>
				Result = SUT.Compare(Context.Object, value1, value2)
			);

			"Then it should call CanCompare on the first inner comparisons".x(() =>
				Inner[0].Verify(c => c.CanCompare(typeof(object), typeof(object)), Times.Once())
			);

			"and it should call Compare on the first inner comparison".x(() =>
				Inner[0].Verify(c => c.Compare(Context.Object, value1, value2), Times.Once())
			);

			"but it shouldn't call the other comparisons CanCompare".x(() =>
				Inner.Skip(1).VerifyAll(c => c.CanCompare(It.IsAny<Type>(), It.IsAny<Type>()), Times.Never())
			);

			"and it shouldn't call the other comparisons Compare".x(() =>
				Inner.Skip(1).VerifyAll(c => c.Compare(It.IsAny<IComparisonContext>(), It.IsAny<object>(), It.IsAny<object>()), Times.Never())
			);

			"and it should return Pass".x(() =>
				Result.ShouldBe(ComparisonResult.Pass)
			);
		}

		[Scenario]
		public void When_testing_equality_if_all_comparers_cant_compare_the_values(object value1, object value2)
		{
			"Given some values to compare".x(() =>
			{
				value1 = Fixture.Create<object>();
				value2 = Fixture.Create<object>();
			});

			"And a Comparison context object".x(() =>
				Context = Fixture.Create<Mock<IComparisonContext>>()
			);

			"When calling Compare".x(() =>
				Result = SUT.Compare(Context.Object, value1, value2)
			);

			"it should call the inner comparers CanCompare".x(() =>
				Inner.VerifyAll(c => c.CanCompare(typeof(object), typeof(object)), Times.Once())
			);

			"it should not call the inner comparers Compare".x(() =>
				Inner.VerifyAll(c => c.Compare(Context.Object, value1, value2), Times.Never())
			);

			"and it should return false".x(() =>
				Result.ShouldBe(ComparisonResult.Inconclusive)
			);
		}

		[Scenario]
		[Example(null, 1, ComparisonResult.Fail)]
		[Example(1, null, ComparisonResult.Fail)]
		[Example(null, null, ComparisonResult.Pass)]
		public void When_testing_nulls(object value1, object value2, ComparisonResult expected)
		{
			"Given a Comparison context object".x(() =>
				Context = Fixture.Create<Mock<IComparisonContext>>()
			);

			"When calling Compare".x(() =>
				Result = SUT.Compare(Context.Object, value1, value2)
			);

			"Then it should return {2}".x(() =>
				Result.ShouldBe(expected)
			);

			"And it should not call the inner comparers CanCompare".x(() =>
				Inner.VerifyAll(c => c.CanCompare(It.IsAny<Type>(), It.IsAny<Type>()), Times.Never())
			);

			"And it should not call the inner comparers Compare".x(() =>
				Inner.VerifyAll(c => c.Compare(Context.Object, value1, value2), Times.Never())
			);
		}
	}
}