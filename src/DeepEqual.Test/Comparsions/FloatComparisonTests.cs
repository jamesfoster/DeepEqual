namespace DeepEqual.Test.Comparsions
{
    using Shouldly;

    using System;
    using System.Collections.Generic;

    using Xbehave;

    using Xunit;

    public class FloatComparisonTests
	{
		protected FloatComparison SUT { get; set; }
		protected ComparisonContext Context { get; set; }

		protected ComparisonResult Result { get; set; }
		protected bool CanCompareResult { get; set; }

		[Scenario]
		public void Creating_a_FloatComparison()
		{
			"When creating a FloatComparison".x(() =>
				SUT = new FloatComparison(0.1d, 0.1f)
			);

			"Then is should implement IComparison".x(() =>
				SUT.ShouldBeAssignableTo<IComparison>()
			);
		}

		[Scenario]
		[Example(typeof(float),   typeof(int),    true)]
		[Example(typeof(double),  typeof(int),    true)]
		[Example(typeof(double),  typeof(float),  true)]
		[Example(typeof(byte),    typeof(float),  true)]
		[Example(typeof(char),    typeof(double), false)]
		[Example(typeof(int),     typeof(long),   false)]
		[Example(typeof(decimal), typeof(int),    false)]
		public void Can_compare_float_types(Type type1, Type type2, bool canCompare)
		{
			"Given a FloatComparison".x(() =>
				SUT = new FloatComparison(0.1d, 0.1f)
			);

			"When calling CanCompare".x(() =>
				CanCompareResult = SUT.CanCompare(type1, type2)
			);

			"Then the result should be {2}".x(() =>
				CanCompareResult.ShouldBe(canCompare)
			);
		}

		[Scenario]
		[MemberData(nameof(TestData))]
		public void Compares_floats_within_tolarance(
			double doubleTolerance,
			float singleTolerance,
			object value1,
			object value2,
			ComparisonResult result)
		{
			"Given a FloatComparison".x(() =>
				SUT = new FloatComparison(doubleTolerance, singleTolerance)
			);

			"And a Comparison context object".x(() =>
				Context = new ComparisonContext()
			);

			"When calling Compare".x(() =>
				(Result, _) = SUT.Compare(Context, value1, value2)
			);

			"And it should return Pass".x(() =>
				Result.ShouldBe(result)
			);
		}

		public static IEnumerable<object[]> TestData => new[]
		{
			new object[] {1e-15, 1e-6, 0, 0.0d, ComparisonResult.Pass},
			new object[] {1e-15, 1e-6, 0.0f, 0, ComparisonResult.Pass},
			new object[] {1e-15, 1e-6, double.Epsilon, 0.0d, ComparisonResult.Pass},
			new object[] {1e-15, 1e-6, 0.0f, float.Epsilon, ComparisonResult.Pass},
			new object[] {0.0d, 0.0f, double.Epsilon, 0.0d, ComparisonResult.Fail},
			new object[] {0.0d, 0.0f, 0.0f, float.Epsilon, ComparisonResult.Fail},
			new object[] {1e-15, 1e-6, 1.111_111_111_111_118d, 1.111_111_111_111_119d, ComparisonResult.Pass},
			new object[] {1e-15, 1e-6, 1.111_111_111_111_117d, 1.111_111_111_111_119d, ComparisonResult.Fail},
			new object[] {1e-15, 1e-6, 1.111_118f, 1.111_119f, ComparisonResult.Pass},
			new object[] {1e-15, 1e-6, 1.111_117f, 1.111_119f, ComparisonResult.Fail},
			new object[] {1e-15, 1e-6, 100_000_100.0f, 100_000_000, ComparisonResult.Pass},
			new object[] {1e-15, 1e-6, 100_000_000.0f, 100_000_200m, ComparisonResult.Fail},
			new object[] {0.0001d, 0.01f, 10000.0d, 10001, ComparisonResult.Pass},
			new object[] {0.0001d, 0.01f, float.NaN, float.NaN, ComparisonResult.Pass},
			new object[] {0.0001d, 0.01f, double.NaN, double.NaN, ComparisonResult.Pass},
			new object[] {0.0001d, 0.01f, 0.0f, float.NaN, ComparisonResult.Fail},
			new object[] {0.0001d, 0.01f, double.NaN, 0.001d, ComparisonResult.Fail}
		};
	}
}
