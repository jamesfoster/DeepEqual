namespace DeepEqual.Test
{
	using System;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;

	using ApprovalTests;
	using ApprovalTests.Reporters;

	using DeepEqual.Formatting;
	using DeepEqual.Syntax;
	using DeepEqual.Test.Helper;

	using Xunit;

	public class DeepComparisonTest
	{
		[Fact]
		public void KitchenSink()
		{
			var object1 = new
				{
					A = 1,
					B = UriKind.Absolute,
					C = new List<int> {1, 2, 3},
					Float = 1.111_111_8f,
					Double = 1.111_111_111_111_118d,
					Inner = new
						{
							X = 1,
							Y = 2,
							Z = 3
						},
					Set = new[] {3, 4, 2, 1},
					Dictionary = new Dictionary<int, int>
						{
							{2, 3},
							{123, 234},
							{345, 456}
						}
				};

			var object2 = new
				{
					A = 1,
					B = "Absolute",
					C = new[] {1, 2, 3},
					Float = 1.111_111_9m,
					Double = 1.111_111_111_111_119m,
					Inner = new TestType
						{
							X = 1,
							Y = 3,
							Z = 3
						},
					Set = new HashSet<int> {1, 2, 3, 4},
					Dictionary = new Dictionary<int, int>
						{
							{123, 234},
							{345, 456},
							{2, 3}
						}
				};

			var comparison = new ComparisonBuilder()
				.IgnoreProperty<TestType>(x => x.Y)
				.Create();

			DeepAssert.AreEqual(object1, object2, comparison);
		}

		[Fact]
		[UseReporter(typeof(DiffReporter))]
		public void KitchenSinkFailures()
		{
			var object1 = new
			{
				A = 1,
				B = UriKind.Absolute,
				C = new List<int> { 1, 2, 3 },
				Float = 1.111_111_6f,
				Double = 1.111_111_111_111_116d,
				String = "a1b2c3",
				Inner = new
				{
					X = 1,
					Y = 2,
					Z = 3
				},
				Set = new[] { 3, 4, 2, 1 },
				Dictionary = new Dictionary<int, int>
				{
					{2, 3},
					{123, 234},
					{345, 456}
				}
			};

			var object2 = new
			{
				A = 2,
				B = "Not Quite Absolute",
				C = new[] { 3, 2, 1 },
				Float = 1.111_111_9m,
				Double = 1.111_111_111_111_119m,
				String = new Regex("^(abc)\\d+$"),
				Inner = new TestType
				{
					X = 1,
					Y = 3,
					Z = 3
				},
				Set = new HashSet<int> { 1, 2, 3, 5 },
				Dictionary = new Dictionary<int, int>
				{
					{123, 2345},
					{34, 456},
					{2, 3}
				}
			};

			var syntax = object1
				.WithDeepEqual(object2)
				.WithCustomComparison(new RegexComparison())
				.WithCustomFormatter<RegexDifference>(new RegexDifferenceFormatter());

			var exception = Assert.Throws<DeepEqualException>(() => syntax.Assert());

			Approvals.Verify(exception.Message);
		}
	}

	public class TestType 
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }
	}

	public class RegexComparison : IComparison
	{
		public bool CanCompare(Type type1, Type type2)
		{
			return type1 == typeof(string) && type2 == typeof(Regex);
		}

		public (ComparisonResult result, IComparisonContext context) Compare(IComparisonContext context, object value1, object value2)
		{
			var str = (string) value1;
			var regex = (Regex) value2;

			if (regex.IsMatch(str))
				return (ComparisonResult.Pass, context);

			return (ComparisonResult.Fail, context.AddDifference(new RegexDifference(context.Breadcrumb, str, regex)));
		}
	}

	public class RegexDifference : Difference
	{
		public string Value { get; }
		public Regex Regex { get; }

		public RegexDifference(string breadcrumb, string value, Regex regex) : base(breadcrumb)
		{
			Value = value;
			Regex = regex;
		}
	}

	public class RegexDifferenceFormatter : IDifferenceFormatter
	{
		public string Format(Difference difference)
		{
			var regexDiff = (RegexDifference) difference;

			return $"Actual{regexDiff.Breadcrumb} doesn't match regex {regexDiff.Regex}\n{regexDiff.Value}";
		}
	}
}