using Xunit.Extensions;

namespace DeepEqual.Test
{
	using System.Collections.Generic;

	using DeepEqual.Formatting;

	using Xunit;

	public class ExceptionMessageTests
	{
		[Fact]
		public void No_differences_recorded()
		{
			var context = new ComparisonContext();

			AssertExceptionMessage(context, "Comparison Failed");
		}

		[Fact]
		public void null_difference()
		{
			var context = new ComparisonContext();
			context.AddDifference(null, new object());

			AssertExceptionMessage(context, @"
Comparison Failed: The following 1 differences were found.
	Actual != Expected ((null) != System.Object)");
		}

		[Fact]
		public void Single_string_difference()
		{
			var context = new ComparisonContext();
			context.AddDifference("a", "b");

			AssertExceptionMessage(context, @"
Comparison Failed: The following 1 differences were found.
	Actual != Expected (""a"" != ""b"")");
		}

		[Fact]
		public void Single_int_difference()
		{
			var context = new ComparisonContext();
			context.AddDifference(1, 2);

			AssertExceptionMessage(context, @"
Comparison Failed: The following 1 differences were found.
	Actual != Expected (1 != 2)");
		}

		[Fact]
		public void List_int_difference()
		{
			var context = new ComparisonContext();
			var innerContext = context.VisitingIndex(2);
			innerContext.AddDifference(1, 2);
			innerContext = context.VisitingIndex(4);
			innerContext.AddDifference(4, 5);

			AssertExceptionMessage(context, @"
Comparison Failed: The following 2 differences were found.
	Actual[2] != Expected[2] (1 != 2)
	Actual[4] != Expected[4] (4 != 5)");
		}

		[Fact]
		public void Long_string_difference_start()
		{
			var context = new ComparisonContext();
			context.AddDifference(
				"01234567890123456789012345678901234567890123456789",
				"abcdefghijabcdefghijabcdefghijabcdefghijabcdefghij");

			AssertExceptionMessage(context, @"
Comparison Failed: The following 1 differences were found.
	Actual != Expected (""01234567890123456789..."" != ""abcdefghijabcdefghij..."")");
		}

		[Fact]
		public void Long_string_difference_middle()
		{
			var context = new ComparisonContext();
			context.AddDifference(
				"01234567890123456789012345first678901234567890123456789",
				"01234567890123456789012345second678901234567890123456789");

			AssertExceptionMessage(context, @"
Comparison Failed: The following 1 differences were found.
	Actual != Expected (""...6789012345first67890..."" != ""...6789012345second6789..."")");
		}

		[Fact]
		public void Long_string_difference_end()
		{
			var context = new ComparisonContext();
			context.AddDifference(
				"01234567890123456789012345678901234567890123456789a",
				"01234567890123456789012345678901234567890123456789b");

			AssertExceptionMessage(context, @"
Comparison Failed: The following 1 differences were found.
	Actual != Expected (""...1234567890123456789a"" != ""...1234567890123456789b"")");
		}

		[Fact]
		public void Long_string_difference_end_2()
		{
			var context = new ComparisonContext();
			context.AddDifference(
				"01234567890123456789012345678901234567890123",
				"01234567890123456789012345678901234567890123456789");

			AssertExceptionMessage(context, @"
Comparison Failed: The following 1 differences were found.
	Actual != Expected (""...45678901234567890123"" != ""...01234567890123456789"")");
		}

		[Theory]
		[InlineData("0123456789012345678", "01234567890123456789", "0123456789012345678", "01234567890123456789")]
		[InlineData("012345678901234567", "0123456789012345678", "012345678901234567", "0123456789012345678")]
		[InlineData("0123456789012345678", "012345678901234567890", "0123456789012345678", "...12345678901234567890")]
		[InlineData("012345678901234567890", "0123456789012345678", "...12345678901234567890", "0123456789012345678")]
		public void Strings_around_same_length_as_max_length(string value1, string value2, string expected1, string expected2)
		{
			var context = new ComparisonContext();
			context.AddDifference(
				value1,
				value2);

			AssertExceptionMessage(context, string.Format(@"
Comparison Failed: The following 1 differences were found.
	Actual != Expected (""{0}"" != ""{1}"")", expected1, expected2));
		}

		[Fact]
		public void Missing_expected_entry()
		{
			var context = new ComparisonContext();
			context.AddDifference(new MissingEntryDifference(MissingSide.Expected, "Index", "Value"));

			AssertExceptionMessage(context, @"
Comparison Failed: The following 1 differences were found.
	Expected[""Index""] not found (Actual[""Index""] = ""Value"")");
		}

		[Fact]
		public void Missing_actual_entry()
		{
			var context = new ComparisonContext();
			context.AddDifference(new MissingEntryDifference(MissingSide.Actual, "Index", "Value"));

			AssertExceptionMessage(context, @"
Comparison Failed: The following 1 differences were found.
	Actual[""Index""] not found (Expected[""Index""] = ""Value"")");
		}

		[Fact]
		public void Set_difference_expected()
		{
			var context = new ComparisonContext();
			context.AddDifference(new SetDifference(".Set", new List<object>{1,2,3}, new List<object>()));

			AssertExceptionMessage(context, @"
Comparison Failed: The following 1 differences were found.
	Actual.Set != Expected.Set
		Expected.Set contains the following unmatched elements:
			1
			2
			3");
		}

		[Fact]
		public void Set_difference_actual()
		{
			var context = new ComparisonContext();
			context.AddDifference(new SetDifference(".Set", new List<object>(), new List<object>{1,2,3}));

			AssertExceptionMessage(context, @"
Comparison Failed: The following 1 differences were found.
	Actual.Set != Expected.Set
		Actual.Set contains the following unmatched elements:
			1
			2
			3");
		}

		private static void AssertExceptionMessage(ComparisonContext context, string expectedMessage)
		{
			expectedMessage = expectedMessage.Trim().Replace("\r\n", "\n");

			var messageBuilder = new DeepEqualExceptionMessageBuilder(context);

			var message = messageBuilder.GetMessage().Replace("\r\n", "\n");

			Assert.Equal(expectedMessage, message);
		}
	}
}