namespace DeepEqual.Test.Comparsions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DeepEqual;

	using Moq;

	using Ploeh.AutoFixture;
	using Ploeh.AutoFixture.AutoMoq;

	using Shouldly;

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
			"Given a fixture"
				.Given(() =>
					{
						Fixture = new Fixture();
						Fixture.Customize(new AutoMoqCustomization());
					});

			"And some mock comparers"
				.And(() =>
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

			"And a CompositeComparer"
				.And(() => SUT = Fixture.Build<CompositeComparison>()
				                        .With(x => x.Comparisons)
				                        .Create());
		}

		[Scenario]
		public void When_creating_a_CompositeComparer()
		{
			"When creating a CompostieComperer"
				.When(() => SUT = new CompositeComparison());

			"it should implement IComparer"
				.Then(() => SUT.ShouldBeTypeOf<IComparison>());

			"CanCompare should always true"
				.And(() => SUT.CanCompare(null, null).ShouldBe(true));
		}

		[Scenario]
		public void When_testing_equality_if_a_comparer_returns_Inconclusive(object value1, object value2)
		{
			"Given the first comparer can compare the values"
				.Given(() => Inner[0]
					             .Setup(c => c.CanCompare(It.IsAny<Type>(), It.IsAny<Type>()))
					             .Returns(true));

			"... and returns Inconclusive"
				.And(() => Inner[0]
					           .Setup(c => c.Compare(It.IsAny<IComparisonContext>(), It.IsAny<object>(), It.IsAny<object>()))
					           .Returns(ComparisonResult.Inconclusive));

			"And some values to compare"
				.And(() =>
					{
						value1 = new object();
						value2 = new object();
					});

			"And a Comparison context object"
				.And(() => Context = Fixture.Create<Mock<IComparisonContext>>());

			"When calling Compare"
				.When(() => Result = SUT.Compare(Context.Object, value1, value2));

			"Then it should call CanCompare on all inner comparisons"
				.Then(() => Inner.VerifyAll(c => c.CanCompare(typeof (object), typeof (object)), Times.Once()));

			"and it should call Compare on the first inner comparison"
				.And(() => Inner[0].Verify(c => c.Compare(Context.Object, value1, value2), Times.Once()));

			"but it shouldn't call the other comparisons Compare"
				.But(() => Inner.Skip(1).VerifyAll(c => c.Compare(It.IsAny<IComparisonContext>(), It.IsAny<object>(), It.IsAny<object>()), Times.Never()));

			"and it should return Inconclusive"
				.And(() => Result.ShouldBe(ComparisonResult.Inconclusive));
		}

		[Scenario]
		public void When_testing_equality_if_a_comparer_returns_Pass(object value1, object value2)
		{
			"Given the first comparer can compare the values"
				.Given(() => Inner[0]
					             .Setup(c => c.CanCompare(It.IsAny<Type>(), It.IsAny<Type>()))
					             .Returns(true));

			"... and returns Pass"
				.And(() => Inner[0]
					           .Setup(c => c.Compare(It.IsAny<IComparisonContext>(), It.IsAny<object>(), It.IsAny<object>()))
					           .Returns(ComparisonResult.Pass));

			"And some values to compare"
				.And(() =>
					{
						value1 = new object();
						value2 = new object();
					});

			"And a Comparison context object"
				.And(() => Context = Fixture.Create<Mock<IComparisonContext>>());

			"When calling Compare"
				.When(() => Result = SUT.Compare(Context.Object, value1, value2));

			"Then it should call CanCompare on the first inner comparisons"
				.Then(() => Inner[0].Verify(c => c.CanCompare(typeof (object), typeof (object)), Times.Once()));

			"and it should call Compare on the first inner comparison"
				.And(() => Inner[0].Verify(c => c.Compare(Context.Object, value1, value2), Times.Once()));

			"but it shouldn't call the other comparisons CanCompare"
				.But(() => Inner.Skip(1).VerifyAll(c => c.CanCompare(It.IsAny<Type>(), It.IsAny<Type>()), Times.Never()));

			"and it shouldn't call the other comparisons Compare"
				.And(() => Inner.Skip(1).VerifyAll(c => c.Compare(It.IsAny<IComparisonContext>(), It.IsAny<object>(), It.IsAny<object>()), Times.Never()));

			"and it should return Pass"
				.And(() => Result.ShouldBe(ComparisonResult.Pass));
		}

		[Scenario]
		public void When_testing_equality_if_all_comparers_cant_compare_the_values(object value1, object value2)
		{
			"Given some values to compare"
				.Given(() =>
					{
						value1 = Fixture.Create<object>();
						value2 = Fixture.Create<object>();
					});

			"And a Comparison context object"
				.And(() => Context = Fixture.Create<Mock<IComparisonContext>>());

			"When calling Compare"
				.When(() => Result = SUT.Compare(Context.Object, value1, value2));

			"it should call the inner comparers CanCompare"
				.Then(() => Inner.VerifyAll(c => c.CanCompare(typeof (object), typeof (object)), Times.Once()));

			"it should call the inner comparers Compare"
				.Then(() => Inner.VerifyAll(c => c.Compare(Context.Object, value1, value2), Times.Never()));

			"and it should return false"
				.And(() => Result.ShouldBe(ComparisonResult.Inconclusive));
		}
	}
}