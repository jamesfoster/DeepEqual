namespace DeepEqual.Test.Comparsions
{
	using System;
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

		public void SetUp()
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
						     .Returns<IComparisonContext, object, object>(
							     (c, v1, v2) => v1.Equals(v2) ? ComparisonResult.Pass : ComparisonResult.Fail);
					});

			"And a ComplexObjectComparison"
				.And(() => SUT = Fixture.Build<ComplexObjectComparison>()
				                        .Without(x => x.IgnoredProperties)
				                        .Create());

			"And a Comparison context object"
				.And(() => Context = new ComparisonContext("Property"));
		}

		[Scenario]
		[PropertyData("SimilarObjectsTestData")]
		public void When_comparing_objects_of_the_same_type(bool ignoreUnmatchedProperties, object value1, object value2, ComparisonResult expected)
		{
			SetUp();

			"And IgnoreUnmatchedProperties is set to {0}"
				.And(() => SUT.IgnoreUnmatchedProperties = ignoreUnmatchedProperties);

			"When calling Compare"
				.When(() => Result = SUT.Compare(Context, value1, value2));

			"It should return {3}"
				.Then(() => Result.ShouldBe(expected));

			"And it should call Compare on the inner comparison for each property"
				.And(() =>
					{
						var properties1 = value1.GetType().GetProperties();
						var properties2 = value2.GetType().GetProperties().ToDictionary(x => x.Name);

						foreach (var p1 in properties1)
						{
							if(!properties2.ContainsKey(p1.Name))
								continue;

							var p2 = properties2[p1.Name];

							var v1 = p1.GetValue(value1);
							var v2 = p2.GetValue(value2);
							Inner.Verify(x => x.Compare(It.Is<IComparisonContext>(c => c.Breadcrumb == "Property." + p1.Name), v1, v2), Times.Once());
						}
					});
		}

		[Scenario]
		public void Ignoring_properties_on_the_source_type()
		{
			MyClass value1 = null;
			object value2 = null;

			SetUp();

			"And the IgnoreMe property is ignored"
				.And(() => SUT.IgnoreProperty<MyClass>(x => x.IgnoreMe));

			"And value1 is a MyClass instance"
				.And(() => value1 = new MyClass
					{
						A = Fixture.Create<string>(),
						B = Fixture.Create<string>(),
						IgnoreMe = Fixture.Create<string>()
					});

			"And value2 is equivalent to value1"
				.And(() => value2 = new
					{
						value1.A,
						value1.B,
						IgnoreMe = Fixture.Create<string>()
					});

			"When comparing the 2 values"
				.When(() => Result = SUT.Compare(Context, value1, value2));

			"Then it should return a Pass"
				.Then(() => Result.ShouldBe(ComparisonResult.Pass));
		}

		[Scenario]
		public void Ignoring_properties_on_the_destination_type()
		{
			object value1 = null;
			MyClass value2 = null;

			SetUp();

			"And the IgnoreMe property is ignored"
				.And(() => SUT.IgnoreProperty<MyClass>(x => x.IgnoreMe));

			"And value1 is a MyClass instance"
				.And(() => value2 = new MyClass
					{
						A = Fixture.Create<string>(),
						B = Fixture.Create<string>(),
						IgnoreMe = Fixture.Create<string>()
					});

			"And value2 is equivalent to value1"
				.And(() => value1 = new
					{
						value2.A,
						value2.B,
						IgnoreMe = Fixture.Create<string>()
					});

			"When comparing the 2 values"
				.When(() => Result = SUT.Compare(Context, value1, value2));

			"Then it should return a Pass"
				.Then(() => Result.ShouldBe(ComparisonResult.Pass));
		}

		private class MyClass
		{
			public string A { get; set; }
			public string B { get; set; }
			public string IgnoreMe { get; set; }
		}

		public static IEnumerable<object[]> SimilarObjectsTestData
		{
			get
			{
				return new[]
					{
						new object[]
							{
								false,
								new {A = 1, B = 2, C = 3},
								new {A = 1, B = 2, C = 3},
								ComparisonResult.Pass
							},
						new object[]
							{
								false,
								new {A = 1, B = 2, C = 3},
								new {A = 1, B = 2},
								ComparisonResult.Fail
							},
						new object[]
							{
								false,
								new {A = 1, B = 2},
								new {A = 1, B = 2, C = 3},
								ComparisonResult.Fail
							},
						new object[]
							{
								false,
								new {A = 1, B = 2, C = 3},
								new {A = 123, B = 2, C = 3},
								ComparisonResult.Fail
							},
						new object[]
							{
								true,
								new {A = 1, B = 2, C = 3},
								new {A = 1, B = 2, C = 3},
								ComparisonResult.Pass
							},
						new object[]
							{
								true,
								new {A = 1, B = 2, C = 3},
								new {A = 1, B = 2},
								ComparisonResult.Pass
							},
						new object[]
							{
								true,
								new {A = 1, B = 2},
								new {A = 1, B = 2, C = 3},
								ComparisonResult.Pass
							},
						new object[]
							{
								true,
								new {A = 1, B = 2, C = 3},
								new {A = 123, B = 2, C = 3},
								ComparisonResult.Fail
							}
					};
			}
		} 
	}
}