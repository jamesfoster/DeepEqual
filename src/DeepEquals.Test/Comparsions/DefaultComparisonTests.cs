namespace DeepEquals.Test.Comparsions
{
	using System;
	using System.Collections.Generic;

	using Moq;

	using Ploeh.AutoFixture;

	using Xbehave;

	using Shouldly;

	public class DefaultComparisonTests
	{
		protected DefaultComparison SUT { get; set; }
		protected Mock<IComparisonContext> Context { get; set; }

		protected ComparisonResult Result { get; set; }
		protected bool CanCompareResult { get; set; }

		[Scenario]
		public void Creating_a_DefaultComparison()
		{
			"When creating a DefaultComparison"
				.When(() => SUT = new DefaultComparison());

			"Then is should implement IComparison"
				.Then(() => SUT.ShouldBeTypeOf<IComparison>());
		}

		[Scenario]
		[Example(typeof(int))]
		[Example(typeof(string))]
		[Example(typeof(object))]
		[Example(typeof(Type))]
		public void Can_compare_types_that_are_equal(Type type)
		{
			"Given a DefaultComparison"
				.Given(() => SUT = new DefaultComparison());

			"When calling CanCompare"
				.When(() => CanCompareResult = SUT.CanCompare(type, type));

			"Then the result should be true"
				.Then(() => CanCompareResult.ShouldBe(true));
		}

		[Scenario]
		[Example(typeof(int), typeof(object))]
		public void Can_not_compare_types_that_are_different(Type type1, Type type2)
		{
			"Given a DefaultComparison"
				.Given(() => SUT = new DefaultComparison());

			"When calling CanCompare"
				.When(() => CanCompareResult = SUT.CanCompare(type1, type2));

			"Then the result should be true"
				.Then(() => CanCompareResult.ShouldBe(false));
		}

		[Scenario]
		public void Comparing_objects_calls_Equals_method()
		{
			var object1 = default (EqualsSpy);
			var object2 = default (EqualsSpy);

			"Given a DefaultComparison"
				.Given(() => SUT = new DefaultComparison());

			"And 2 objects to compare"
				.And(() =>
					{
						object1 = new EqualsSpy();
						object2 = new EqualsSpy();
					});

			"And a Comparison context object"
				.And(() => Context = new Fixture().Create<Mock<IComparisonContext>>());

			"When calling Compare"
				.When(() => Result = SUT.Compare(Context.Object, object1, object2));

			"Then it should call Equals"
				.Then(() => object1.Calls[0].ShouldBeSameAs(object2));
		}

		private class EqualsSpy
		{
			public List<object> Calls { get; private set; }

			public EqualsSpy()
			{
				Calls = new List<object>();
			}

			public override bool Equals(object obj)
			{
				Calls.Add(obj);

				return base.Equals(obj);
			}

			public override int GetHashCode()
			{
				return (Calls != null ? Calls.GetHashCode() : 0);
			}
		}
	}
}