namespace DeepEqual.Test.Comparsions
{
	using System;
	using System.Collections.Generic;

	using Xbehave;

	using Shouldly;

	public class DefaultComparisonTests
	{
		protected DefaultComparison SUT { get; set; }
		protected ComparisonContext Context { get; set; }

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
		[Example(typeof(int), typeof(long))]
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
				.And(() => Context = new ComparisonContext());

			"When calling Compare"
				.When(() => Result = SUT.Compare(Context, object1, object2));

			"Then it should call Equals"
				.Then(() => object1.Calls[0].ShouldBeSameAs(object2));
		}

		[Scenario]
		[Example(1, 1, true)]
		[Example(1, 2, false)]
		[Example(2, 2, true)]
		[Example("a", "a", true)]
		[Example("a", "b", false)]
		public void Comparing_value_types_returns_Pass_or_Fail(object value1, object value2, bool expected)
		{
			"Given a DefaultComparison"
				.Given(() => SUT = new DefaultComparison());
			
			"And a Comparison context object"
				.And(() => Context = new ComparisonContext("Root"));

			"When calling Compare"
				.When(() => Result = SUT.Compare(Context, value1, value2));

			"The result should be Pass or Fail"
				.Then(() => Result.ShouldBe(expected ? ComparisonResult.Pass : ComparisonResult.Fail));

			if (!expected)
			{
				var expectedDifference = new Difference
					{
						Breadcrumb = "Root",
						Value1 = value1,
						Value2 = value2
					};

				"And it should add a difference"
					.And(() => Context.Differences[0].ShouldBe(expectedDifference));
			}
		}

		[Scenario]
		[Example(true)]
		[Example(false)]
		public void Comparing_object_types_returns_Pass_or_Inconclusive(bool expected)
		{
			var value1 = default (EqualsSpy);
			var value2 = default (EqualsSpy);

			"Given a DefaultComparison"
				.Given(() => SUT = new DefaultComparison());

			"And 2 objects to compare"
				.And(() =>
					{
						value1 = new EqualsSpy(expected);
						value2 = new EqualsSpy(expected);
					});

			"When calling Compare"
				.When(() => Result = SUT.Compare(null, value1, value2));

			"The result should be Pass or Fail"
				.Then(() => Result.ShouldBe(expected ? ComparisonResult.Pass : ComparisonResult.Inconclusive));
		}

		[Scenario]
		[Example(typeof(AlwaysEqual), typeof(object))]
		[Example(typeof(object), typeof(AlwaysEqual))]
		[Example(typeof(AlwaysEqual), typeof(AlwaysEqual))]
		[Example(typeof(object), typeof(AlwaysEqualAswell))]
		[Example(typeof(AlwaysEqualAswell), typeof(object))]
		public void Calling_CanCompare_compare_on_ignored_types(Type type1, Type type2)
		{
			"Given a DefaultComparison"
				.Given(() => SUT = new DefaultComparison());

			"And the type is skipped"
				.And(() => SUT.Skip<AlwaysEqual>());

			"When calling Compare"
				.When(() => CanCompareResult = SUT.CanCompare(type1, type2));

			"The result should be Pass or Fail"
				.Then(() => CanCompareResult.ShouldBe(false));
		}

		[Scenario]
		public void Calling_Compare_compare_on_ignored_types()
		{
			var value1 = default (AlwaysEqual);
			var value2 = default (object);

			"Given a DefaultComparison"
				.Given(() => SUT = new DefaultComparison());

			"And the type is skipped"
				.And(() => SUT.Skip<AlwaysEqual>());

			"And 2 objects to compare"
				.And(() =>
					{
						value1 = new AlwaysEqual();
						value2 = new AlwaysEqual();
					});

			"When calling Compare"
				.When(() => Result = SUT.Compare(null, value1, value2));

			"The result should be Pass or Fail"
				.Then(() => Result.ShouldBe(ComparisonResult.Inconclusive));
		}

		private class AlwaysEqual
		{
			public override bool Equals(object obj)
			{
				return true;
			}
		}

		private class AlwaysEqualAswell : AlwaysEqual {}

		private class EqualsSpy
		{
			private bool Result { get; set; }
			public List<object> Calls { get; private set; }

			public EqualsSpy(bool result = false)
			{
				Result = result;
				Calls = new List<object>();
			}

			public override bool Equals(object obj)
			{
				Calls.Add(obj);

				return Result;
			}

			public override int GetHashCode()
			{
				return (Calls != null ? Calls.GetHashCode() : 0);
			}
		}
	}
}