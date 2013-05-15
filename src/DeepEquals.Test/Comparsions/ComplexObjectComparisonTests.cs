namespace DeepEquals.Test.Comparsions
{
	using System.Collections.Generic;

	using Moq;

	using Ploeh.AutoFixture;
	using Ploeh.AutoFixture.AutoMoq;

	using Xbehave;

	using Shouldly;

	using Xunit.Extensions;

	using System.Linq;

	public class ComplexObjectComparisonTests
	{
		protected Fixture Fixture { get; set; }
		
		protected ComplexObjectComparison SUT { get; set; }
		protected Mock<IComparison> Inner { get; set; }
		protected ComparisonContext Context { get; set; }

		protected ComparisonResult Result { get; set; }

		[Scenario]
		public void Creating_a_ComplexObjectComparer()
		{
			"When creating a ComplexObjectComparison"
				.When(() => SUT = new ComplexObjectComparison(null));

			"It should be an IComparison"
				.Then(() => SUT.ShouldBeTypeOf<IComparison>());
		}

		[Scenario]
		[PropertyData("SimilarObjectsTestData")]
		public void When_comparing_objects_of_the_same_type(object value1, object value2, ComparisonResult expected)
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

						Inner.Setup(x => x.Compare(It.IsAny<IComparisonContext>(), It.IsAny<object>(), It.IsAny<object>()))
							.Returns<IComparisonContext, object, object>((c, v1, v2) => v1.Equals(v2) ? ComparisonResult.Pass : ComparisonResult.Fail);
					});

			"And a ComplexObjectComparison"
				.And(() => SUT = Fixture.Create<ComplexObjectComparison>());

			"And a Comparison context object"
				.And(() => Context = new ComparisonContext("Property"));

			"When calling Compare"
				.When(() => Result = SUT.Compare(Context, value1, value2));

			"It should return {2}"
				.Then(() => Result.ShouldBe(expected));

			"And it should call Compare on the inner comparison for each property"
				.And(() =>
					{
						var properties1 = value1.GetType().GetProperties();
						var properties2 = value2.GetType().GetProperties();

						foreach (var p1 in properties1)
						{
							var p2 = properties2.FirstOrDefault(p => p.Name == p1.Name);

							var v1 = p1.GetValue(value1);
							var v2 = p2.GetValue(value2);
							Inner.Verify(x => x.Compare(It.Is<IComparisonContext>(c => c.Breadcrumb == "Property." + p1.Name), v1, v2), Times.Once());
						}
					});
		}

		public static IEnumerable<object[]> SimilarObjectsTestData
		{
			get
			{
				return new[]
					{
						new object[]
							{
								new {A = 1, B = 2, C = 3},
								new {A = 1, B = 2, C = 3},
								ComparisonResult.Pass
							},
							new object[]
							{
								new {A = 1, B = 2, C = 3},
								new {A = 123, B = 2, C = 3},
								ComparisonResult.Fail
							}

					};
			}
		} 
	}
}
